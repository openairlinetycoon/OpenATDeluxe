"""
	Sound Bank by Yui Kinomoto @arlez80
"""

const drum_track_bank = 128
const SoundFont = preload( "SoundFont.gd" )
const default_instrument = {
	"mix_rate": 44100,
	"stream": null,
	"ads_state": [
		{ "time": 0, "volume_db": 0.0 },
		{ "time": 0.2, "volume_db": -144.0 },
	],
	"release_state": [
		{ "time": 0, "volume_db": 0.0 },
		{ "time": 0.01, "volume_db": -144.0 },
	],
	"preset": null,
	# "assine_group": 0,	# reserved
}
const default_preset = {
	"name": "",
	"number": 0,
	"instruments": [null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null],
	"bags": [],
}
const default_mix_rate_table = [819,868,920,974,1032,1094,1159,1228,1301,1378,1460,1547,1639,1736,1840,1949,2065,2188,2318,2456,2602,2756,2920,3094,3278,3473,3679,3898,4130,4375,4635,4911,5203,5513,5840,6188,6556,6945,7358,7796,8259,8751,9271,9822,10406,11025,11681,12375,13111,13891,14717,15592,16519,17501,18542,19644,20812,22050,23361,24750,26222,27781,29433,31183,33038,35002,37084,39289,41625,44100,46722,49501,52444,55563,58866,62367,66075,70004,74167,78577,83250,88200,93445,99001,104888,111125,117733,124734,132151,140009,148334,157155,166499,176400,186889,198002,209776,222250,235466,249467,264301,280018,296668,314309,332999,352800,373779,396005,419552,444500,470932,498935,528603,560035,593337,628618,665998,705600,747557,792009,839105,889000,941863,997869,1057205,1120070,1186673,1257236]

# 音色テーブル
var presets = {}

"""
	再生周波数計算
"""
func calc_mix_rate( rate:float, center_key:int, target_key:int ):
	return int( round( rate * pow( 2.0, float( target_key - center_key ) / 12.0 ) ) )

"""
	追加
"""
func set_preset_sample( program_number:int, base_sample:int, base_mix_rate:int ):
	var mix_rate_table = default_mix_rate_table

	if base_mix_rate != 44100:
		print( "not implemented" )
		breakpoint

	var preset = self.default_preset.duplicate( true )
	preset.name = "#%03d" % program_number
	preset.number = program_number
	for i in range(0,128):
		var inst = self.default_instrument.duplicate( true )
		inst.mix_rate = mix_rate_table[i]
		inst.stream = base_sample
		inst.preset = preset
		preset.instruments[i] = inst

	self.set_preset( program_number, preset )

"""
	追加
"""
func set_preset( program_number:int, preset ):
	self.presets[program_number] = preset

"""
	指定した楽器を取得
"""
func get_preset( program_number:int, bank:int = 0 ):
	var pc = program_number | ( bank << 7 )

	# 存在しない場合
	if not self.presets.has( pc ):
		if self.drum_track_bank == bank:
			# ドラムの場合（Standard Kitを選択）
			pc = self.drum_track_bank << 7
		else:
			# 通常楽器の場合（Bank #0を選択）
			pc = program_number
		# それでも存在しない場合
		if not self.presets.has( pc ):
			# 一番最初のデフォルト音源を読む
			pc = self.presets.keys( )[0]

	var preset = self.presets[pc]
	return preset

"""
	サウンドフォント読み込み
"""
func read_soundfont( sf, need_program_numbers = null ):
	var sf_insts = self._read_soundfont_pdta_inst( sf )

	var bag_index:int = 0
	var gen_index:int = 0
	for phdr_index in range( 0, len( sf.pdta.phdr )-1 ):
		var phdr = sf.pdta.phdr[phdr_index]

		var preset = self.default_preset.duplicate( true )
		var program_number:int = phdr.preset | ( phdr.bank << 7 )

		preset.name = phdr.name
		preset.number = program_number

		var bag_next:int = sf.pdta.phdr[phdr_index+1].preset_bag_index
		var bag_count:int = bag_index
		while bag_count < bag_next:
			var gen_next:int = sf.pdta.pbag[bag_count+1].gen_ndx
			var gen_count:int = gen_index
			var bag = {
				"preset": preset,
				"coarse_tune": 0,
				"fine_tune": 0,
				"key_range": null,
				"instrument": null,
				"pan": 0.5,
			}
			while gen_count < gen_next:
				var gen = sf.pdta.pgen[gen_count]
				match gen.gen_oper:
					SoundFont.gen_oper_coarse_tune:
						bag.coarse_tune = gen.amount
					SoundFont.gen_oper_fine_tune:
						bag.fine_tune = gen.amount
					SoundFont.gen_oper_key_range:
						bag.key_range = {
							"high": gen.uamount >> 8,
							"low": gen.uamount & 0xFF,
						}
					SoundFont.gen_oper_pan:
						bag.pan = float( gen.amount + 500 ) / 1000.0
					SoundFont.gen_oper_instrument:
						bag.instrument = sf_insts[gen.uamount]
				gen_count += 1
			if bag.instrument != null:
				preset.bags.append( bag )
			gen_index = gen_next
			bag_count += 1
		bag_index = bag_next

		# 追加するか？
		if need_program_numbers != null:
			if not( program_number in need_program_numbers ) and not( phdr.preset in need_program_numbers ):
				continue
		# 追加
		self._read_soundfont_preset_compose_sample( sf, preset )
		self.set_preset( program_number, preset )

func _read_soundfont_preset_compose_sample( sf, preset ):
	var sample_base = sf.sdta.smpl

	for pbag_index in range( 0, preset.bags.size( ) ):
		var pbag = preset.bags[pbag_index]
		for ibag_index in range( 0, pbag.instrument.bags.size( ) ):
			var ibag = pbag.instrument.bags[ibag_index]
			var start:int = ibag.sample.start + ibag.sample_start_offset
			var end:int = ibag.sample.end + ibag.sample_end_offset
			var start_loop:int = ibag.sample.start_loop + ibag.sample_start_loop_offset
			var end_loop:int = ibag.sample.end_loop + ibag.sample_end_loop_offset
			var mix_rate:float = ibag.sample.sample_rate * pow( 2.0, ( pbag.coarse_tune + ibag.coarse_tune ) / 12.0 ) * pow( 2.0, ( pbag.fine_tune + ibag.sample.pitch_correction + ibag.fine_tune ) / 1200.0 )

			var ass:AudioStreamSample = AudioStreamSample.new( )
			var wave:PoolByteArray = sample_base.subarray( start * 2, end * 2 - 1 )
			ass.data = wave
			ass.format = AudioStreamSample.FORMAT_16_BITS
			ass.mix_rate = int( mix_rate )
			ass.stereo = false #bag.sample.sample_type != SoundFont.sample_link_mono_sample
			ass.loop_begin = start_loop - start
			ass.loop_end = end_loop - start
			if ibag.sample_modes == SoundFont.sample_mode_no_loop or ibag.sample_modes == SoundFont.sample_mode_unused_no_loop:
				ass.loop_mode = AudioStreamSample.LOOP_DISABLED
			else:
				ass.loop_mode = AudioStreamSample.LOOP_FORWARD
			var key_range = ibag.key_range
			if pbag.key_range != null:
				key_range = pbag.key_range

			# ADSRステート生成
			var a:float = ibag.adsr.attack_vol_env_time
			var d:float = ibag.adsr.decay_vol_env_time
			var s:float = ibag.adsr.sustain_vol_env_db
			var r:float = ibag.adsr.release_vol_env_time
			var ads_state = [
				{ "time": 0, "volume_db": -144.0 },
				{ "time": a, "volume_db": 0.0 },
				{ "time": a+d, "volume_db": s },
			]
			var release_state = [
				{ "time": 0, "volume_db": s },
				{ "time": r, "volume_db": -144.0 },
			]
			# 各キーごとに生成
			for key_number in range( key_range.low, key_range.high + 1 ):
				#if preset.number == drum_track_bank << 7:
				#	if 36 <= key_number and key_number <= 40:
				#		print( key_number, " # ", ibag.sample.name );
				if preset.instruments[key_number] != null:
					continue
				var instrument = self.default_instrument.duplicate( true )
				instrument.preset = preset
				if ibag.original_key == 255:
					instrument.mix_rate = ibag.sample.sample_rate
				else:
					instrument.mix_rate = self.calc_mix_rate( mix_rate, ibag.original_key, key_number )
				instrument.stream = ass
	
				instrument.ads_state = ads_state
				instrument.release_state = release_state
				preset.instruments[key_number] = instrument

func _read_soundfont_pdta_inst( sf ):
	var sf_insts = []
	var bag_index:int = 0
	var gen_index:int = 0

	for inst_index in range( 0, len( sf.pdta.inst ) - 1 ):
		var inst = sf.pdta.inst[inst_index]
		var sf_inst = {"name": inst.name, "bags": [] }

		var bag_next:int = sf.pdta.inst[inst_index+1].inst_bag_ndx
		var bag_count:int = bag_index
		var global_bag = {
			"sample": null,
			"sample_id": -1,
			"sample_start_offset": 0,
			"sample_end_offset": 0,
			"sample_start_loop_offset": 0,
			"sample_end_loop_offset": 0,
			"coarse_tune": 0,
			"fine_tune": 0,
			"original_key": 255,
			"keynum": 0,
			"sample_modes": 0,
			"key_range": { "high": 127, "low": 0 },
			"vel_range": { "high": 127, "low": 0 },
			"adsr": {
				"attack_vol_env_time": 0.001,
				"decay_vol_env_time": 0.001,
				"sustain_vol_env_db": 0.0,	# dB
				"release_vol_env_time": 0.001,
			},
		}
		while bag_count < bag_next:
			var bag = global_bag.duplicate( true )
			var gen_next:int = sf.pdta.ibag[bag_count+1].gen_ndx
			var gen_count:int = gen_index
			while gen_count < gen_next:
				var gen = sf.pdta.igen[gen_count]
				match gen.gen_oper:
					SoundFont.gen_oper_key_range:
						bag.key_range.high = gen.uamount >> 8
						bag.key_range.low = gen.uamount & 0xFF
					SoundFont.gen_oper_vel_range:
						bag.vel_range.high = gen.uamount >> 8
						bag.vel_range.low = gen.uamount & 0xFF
					SoundFont.gen_oper_overriding_root_key:
						bag.original_key = gen.amount
					SoundFont.gen_oper_start_addrs_offset:
						bag.sample_start_offset += gen.amount
					SoundFont.gen_oper_end_addrs_offset:
						bag.sample_end_offset += gen.amount
					SoundFont.gen_oper_start_addrs_coarse_offset:
						bag.sample_start_offset += gen.amount * 32768
					SoundFont.gen_oper_end_addrs_coarse_offset:
						bag.sample_end_offset += gen.amount * 32768
					SoundFont.gen_oper_startloop_addrs_offset:
						bag.sample_start_loop_offset += gen.amount
					SoundFont.gen_oper_endloop_addrs_offset:
						bag.sample_end_loop_offset += gen.amount
					SoundFont.gen_oper_startloop_addrs_coarse_offset:
						bag.sample_start_loop_offset += gen.amount * 32768
					SoundFont.gen_oper_endloop_addrs_coarse_offset:
						bag.sample_end_loop_offset += gen.amount * 32768
					SoundFont.gen_oper_coarse_tune:
						bag.coarse_tune = gen.amount
					SoundFont.gen_oper_fine_tune:
						bag.fine_tune = gen.amount
					SoundFont.gen_oper_keynum:
						bag.keynum = gen.amount
					SoundFont.gen_oper_attack_vol_env:
						bag.adsr.attack_vol_env_time = pow( 2.0, float( gen.amount ) / 1200.0 )
					SoundFont.gen_oper_decay_vol_env:
						bag.adsr.decay_vol_env_time = pow( 2.0, float( gen.amount ) / 1200.0 )
					SoundFont.gen_oper_release_vol_env:
						bag.adsr.release_vol_env_time = pow( 2.0, float( gen.amount ) / 1200.0 )
					SoundFont.gen_oper_sustain_vol_env:
						# -144 db == sound font 1440
						var s = min( max( 0.0, float( gen.amount ) ), 1440.0 ) / 10.0
						bag.adsr.sustain_vol_env_db = -s
					SoundFont.gen_oper_sample_modes:
						bag.sample_modes = gen.uamount
					SoundFont.gen_oper_sample_id:
						bag.sample_id = gen.uamount
						bag.sample = sf.pdta.shdr[gen.amount]
						if bag.original_key == 255:
							bag.original_key = bag.sample.original_key
					#_:
					#	print( gen.gen_oper )
				gen_count += 1
			# global zoneでない場合
			if bag.sample != null:
				sf_inst.bags.append( bag )
			else:
				global_bag = bag
			gen_index = gen_next
			bag_count += 1
		sf_insts.append( sf_inst )
		bag_index = bag_next

	return sf_insts
