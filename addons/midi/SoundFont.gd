"""
	SoundFont Reader by Yui Kinomoto @arlez80
"""

"""
	SampleLink
"""
const sample_link_mono_sample = 1
const sample_link_right_sample = 2
const sample_link_left_sample = 4
const sample_link_linked_sample = 8
const sample_link_rom_mono_sample = 0x8001
const sample_link_rom_right_sample = 0x8002
const sample_link_rom_left_sample = 0x8004
const sample_link_rom_linked_sample = 0x8008

"""
	GenerateOperator
"""
const gen_oper_start_addrs_offset = 0
const gen_oper_end_addrs_offset = 1
const gen_oper_startloop_addrs_offset = 2
const gen_oper_endloop_addrs_offset = 3
const gen_oper_start_addrs_coarse_offset = 4
const gen_oper_mod_lfo_to_pitch = 5
const gen_oper_vib_lfo_to_pitch = 6
const gen_oper_mod_env_to_pitch = 7
const gen_oper_initial_filter_fc = 8
const gen_oper_initial_filter_q = 9
const gen_oper_mod_lfo_to_filter_fc = 10
const gen_oper_mod_env_to_filter_fc = 11
const gen_oper_end_addrs_coarse_offset = 12
const gen_oper_mod_lfo_to_volume = 13
const gen_oper_unused1 = 14
const gen_oper_chorus_effects_send = 15
const gen_oper_reverb_effects_send = 16
const gen_oper_pan = 17
const gen_oper_unused2 = 18
const gen_oper_unused3 = 19
const gen_oper_unused4 = 20
const gen_oper_delay_mod_lfo = 21
const gen_oper_freq_mod_lfo = 22
const gen_oper_delay_vib_lfo = 23
const gen_oper_freq_vib_lfo = 24
const gen_oper_delay_mod_env = 25
const gen_oper_attack_mod_env = 26
const gen_oper_hold_mod_env = 27
const gen_oper_decay_mod_env = 28
const gen_oper_sustain_mod_env = 29
const gen_oper_release_mod_env = 30
const gen_oper_keynum_to_mod_env_hold = 31
const gen_oper_keynum_to_mod_env_decay = 32
const gen_oper_delay_vol_env = 33
const gen_oper_attack_vol_env = 34
const gen_oper_hold_vol_env = 35
const gen_oper_decay_vol_env = 36
const gen_oper_sustain_vol_env = 37
const gen_oper_release_vol_env = 38
const gen_oper_keynum_to_vol_env_hold = 39
const gen_oper_keynum_to_vol_env_decay = 40
const gen_oper_instrument = 41
const gen_oper_reserved1 = 42
const gen_oper_key_range = 43
const gen_oper_vel_range = 44
const gen_oper_startloop_addrs_coarse_offset = 45
const gen_oper_keynum = 46
const gen_oper_velocity = 47
const gen_oper_initial_attenuation = 48
const gen_oper_reserved2 = 49
const gen_oper_endloop_addrs_coarse_offset = 50
const gen_oper_coarse_tune = 51
const gen_oper_fine_tune = 52
const gen_oper_sample_id = 53
const gen_oper_sample_modes = 54
const gen_oper_reserved3 = 55
const gen_oper_scale_tuning = 56
const gen_oper_exclusive_class = 57
const gen_oper_overriding_root_key = 58
const gen_oper_unused5 = 59
const gen_oper_end_oper = 60

"""
	SampleMode
"""
const sample_mode_no_loop = 0
const sample_mode_loop_continuously = 1
const sample_mode_unused_no_loop = 2
const sample_mode_loop_ends_by_key_depression = 3

"""
	ファイルから読み込み
	@param	path	File path
	@return	smf
"""
func read_file( path:String ):
	var f:File = File.new( )

	if f.open( path, f.READ ) != OK:
		push_error( "error: cant read file %s" % path )
		breakpoint
	var stream:StreamPeerBuffer = StreamPeerBuffer.new( )
	stream.set_data_array( f.get_buffer( f.get_len( ) ) )
	stream.big_endian = false
	f.close( )

	return self._read( stream )

"""
	配列から読み込み
	@param	data	PoolByteArray
	@return	smf
"""
func read_data( data:PoolByteArray ):
	var stream:StreamPeerBuffer = StreamPeerBuffer.new( )
	stream.set_data_array( data )
	stream.big_endian = false
	return self._read( stream )

"""
	読み込み
	@param	input
	@return	SoundFont
"""
func _read( input:StreamPeerBuffer ):
	self._check_chunk( input, "RIFF" )
	self._check_header( input, "sfbk" )

	var info = self._read_info( input )
	var sdta = self._read_sdta( input )
	var pdta = self._read_pdta( input )

	return {
		"info": info,
		"sdta": sdta,
		"pdta": pdta,
	}

"""
	チャンクチェック
	@param	input
	@param	hdr
"""
func _check_chunk( input:StreamPeerBuffer, hdr:String ):
	self._check_header( input, hdr )
	if input.get_32( ) != 0:
		pass

"""
	ヘッダーチェック
	@param	input
	@param	hdr
"""
func _check_header( input:StreamPeerBuffer, hdr:String ):
	var chk = input.get_string( 4 )
	if hdr != chk:
		print( "Doesn't exist " + hdr + " header" )
		breakpoint

"""
	チャンク読み込み
	@param	input
	@param	needs_header
	@param	chunk
"""
func _read_chunk( stream:StreamPeerBuffer, needs_header = null ):
	var header:String = stream.get_string( 4 )
	if needs_header != null:
		if needs_header != header:
			print( "Doesn't exist " + needs_header + " header" )
			breakpoint
	var size:int = stream.get_u32( )
	var new_stream:StreamPeerBuffer = StreamPeerBuffer.new( )
	new_stream.set_data_array( stream.get_partial_data( size )[1] )
	new_stream.big_endian = false

	return {
		"header": header,
		"size": size,
		"stream": new_stream,
	}

"""
	INFOチャンクを読み込む
	@param	stream
	@param	chunk
"""
func _read_info( stream:StreamPeerBuffer ):
	var chunk = self._read_chunk( stream, "LIST" )
	self._check_header( chunk.stream, "INFO" )

	var info = {
		"ifil":null,
		"isng":null,
		"inam":null,

		"irom":null,
		"iver":null,
		"icrd":null,
		"ieng":null,
		"iprd":null,
		"icop":null,
		"icmt":null,
		"isft":null,
	}

	while 0 < chunk.stream.get_available_bytes( ):
		var sub_chunk = self._read_chunk( chunk.stream )
		match sub_chunk.header.to_lower( ):
			"ifil":
				info.ifil = self._read_version_tag( sub_chunk.stream )
			"isng":
				info.isng = sub_chunk.stream.get_string( sub_chunk.size )
			"inam":
				info.inam = sub_chunk.stream.get_string( sub_chunk.size )
			"irom":
				info.irom = sub_chunk.stream.get_string( sub_chunk.size )
			"iver":
				info.iver = self._read_version_tag( sub_chunk.stream )
			"icrd":
				info.icrd = sub_chunk.stream.get_string( sub_chunk.size )
			"ieng":
				info.ieng = sub_chunk.stream.get_string( sub_chunk.size )
			"iprd":
				info.iprd = sub_chunk.stream.get_string( sub_chunk.size )
			"icop":
				info.icop = sub_chunk.stream.get_string( sub_chunk.size )
			"icmt":
				info.icmt = sub_chunk.stream.get_string( sub_chunk.size )
			"isft":
				info.isft = sub_chunk.stream.get_string( sub_chunk.size )
			_:
				print( "unknown header" )
				breakpoint

	return info

"""
	バージョンタグを読み込む
	@param	stream
	@param	chunk
"""
func _read_version_tag( stream:StreamPeerBuffer ):
	var major:int = stream.get_u16( )
	var minor:int = stream.get_u16( )

	return {
		"major": major,
		"minor": minor,
	}

"""
	SDTAを読み込む
	@param	stream
	@param	chunk
"""
func _read_sdta( stream:StreamPeerBuffer ):
	var chunk = self._read_chunk( stream, "LIST" )
	self._check_header( chunk.stream, "sdta" )

	var smpl = self._read_chunk( chunk.stream, "smpl" )
	var smpl_bytes:PoolByteArray = smpl.stream.get_partial_data( smpl.size )[1]

	var sm24_bytes = null
	if 0 < chunk.stream.get_available_bytes( ):
		var sm24_chunk = self._read_chunk( chunk.stream, "sm24" )
		sm24_bytes = sm24_chunk.stream.get_partial_data( sm24_chunk.size )[1]

	return {
		"smpl": smpl_bytes,
		"sm24": sm24_bytes,
	}

"""
	PDTAを読み込む
	@param	stream
	@param	chunk
"""
func _read_pdta( stream:StreamPeerBuffer ):
	var chunk = self._read_chunk( stream, "LIST" )
	self._check_header( chunk.stream, "pdta" )

	var phdr = self._read_pdta_phdr( chunk.stream )
	var pbag = self._read_pdta_bag( chunk.stream )
	var pmod = self._read_pdta_mod( chunk.stream )
	var pgen = self._read_pdta_gen( chunk.stream )
	var inst = self._read_pdta_inst( chunk.stream )
	var ibag = self._read_pdta_bag( chunk.stream )
	var imod = self._read_pdta_mod( chunk.stream )
	var igen = self._read_pdta_gen( chunk.stream )
	var shdr = self._read_pdta_shdr( chunk.stream )

	return {
		"phdr": phdr,
		"pbag": pbag,
		"pmod": pmod,
		"pgen": pgen,
		"inst": inst,
		"ibag": ibag,
		"imod": imod,
		"igen": igen,
		"shdr": shdr,
	}

"""
	phdr 読み込み
	@param	stream
	@param	chunk
"""
func _read_pdta_phdr( stream:StreamPeerBuffer ):
	var chunk = self._read_chunk( stream, "phdr" )
	var phdrs = []

	while 0 < chunk.stream.get_available_bytes( ):
		var phdr = {
			"name": "",
			"preset": 0,
			"bank": 0,
			"preset_bag_index": 0,
			"library": 0,
			"genre": 0,
			"morphology": 0,
		}

		phdr.name = chunk.stream.get_string( 20 )
		phdr.preset = chunk.stream.get_u16( )
		phdr.bank = chunk.stream.get_u16( )
		phdr.preset_bag_index = chunk.stream.get_u16( )
		phdr.library = chunk.stream.get_32( )
		phdr.genre = chunk.stream.get_32( )
		phdr.morphology = chunk.stream.get_32( )

		phdrs.append( phdr )

	return phdrs

"""
	*bag読み込み
	@param	stream
	@param	chunk
"""
func _read_pdta_bag( stream:StreamPeerBuffer ):
	var chunk = self._read_chunk( stream )
	var bags = []

	if chunk.header.substr( 1, 3 ) != "bag":
		print( "Doesn't exist *bag header." )
		breakpoint

	while 0 < chunk.stream.get_available_bytes( ):
		var bag = {
			"gen_ndx": 0,
			"mod_ndx": 0,
		}
	
		bag.gen_ndx = chunk.stream.get_u16( )
		bag.mod_ndx = chunk.stream.get_u16( )
		bags.append( bag )

	return bags

"""
	*mod読み込み
	@param	stream
	@param	chunk
"""
func _read_pdta_mod( stream:StreamPeerBuffer ):
	var chunk = self._read_chunk( stream )
	var mods = []

	if chunk.header.substr( 1, 3 ) != "mod":
		print( "Doesn't exist *mod header." )
		breakpoint

	while 0 < chunk.stream.get_available_bytes( ):
		var mod = {
			"src_oper": null,
			"dest_oper": 0,
			"amount": 0,
			"amt_src_oper": null,
			"trans_oper": 0,
		}
	
		mod.src_oper = self._read_pdta_modulator( chunk.stream.get_u16( ) )
		mod.dest_oper = chunk.stream.get_u16( )
		mod.amount = chunk.stream.get_u16( )
		mod.amt_src_oper = self._read_pdta_modulator( chunk.stream.get_u16( ) )
		mod.trans_oper = chunk.stream.get_u16( )
		mods.append( mod )

	return mods

"""
	PDTA-Modulator 読み込み
	@param	stream
	@param	chunk
"""
func _read_pdta_modulator( u:int ):
	return {
		"type": ( u >> 10 ) & 0x3f,
		"direction": ( u >> 8 ) & 0x01,
		"polarity": ( u >> 9 ) & 0x01,
		"controller": u & 0x7f,
		"controllerPallete": ( u >> 7 ) & 0x01,
	}

"""
	gen 読み込み
	@param	stream
	@param	chunk
"""
func _read_pdta_gen( stream:StreamPeerBuffer ):
	var chunk = self._read_chunk( stream )
	var chunk_stream = chunk.stream
	var gens = []

	if chunk.header.substr( 1, 3 ) != "gen":
		print( "Doesn't exist *gen header." )
		breakpoint

	while 0 < chunk_stream.get_available_bytes( ):
		var gen_oper:int = chunk_stream.get_u16( )
		var uamount:int = chunk_stream.get_u16( )
		var amount:int = uamount
		if 32767 < uamount: amount = - ( 65536 - uamount )
		gens.append({
			"gen_oper": gen_oper,
			"uamount": uamount,
			"amount": amount,
		})

	return gens

"""
	inst読み込み
	@param	stream
	@param	chunk
"""
func _read_pdta_inst( stream:StreamPeerBuffer ):
	var chunk = self._read_chunk( stream, "inst" )
	var chunk_stream = chunk.stream
	var insts = []

	while 0 < chunk_stream.get_available_bytes( ):
		var inst = {
			"name": "",
			"inst_bag_ndx": 0,
		}
	
		inst.name = chunk_stream.get_string( 20 )
		inst.inst_bag_ndx = chunk_stream.get_u16( )
		insts.append( inst )

	return insts

"""
	shdr 読み込み
	@param	stream
	@param	chunk
"""
func _read_pdta_shdr( stream:StreamPeerBuffer ):
	var chunk = self._read_chunk( stream, "shdr" )
	var shdrs = []

	while 0 < chunk.stream.get_available_bytes( ):
		var shdr = {
			"name": "",
			"start": 0,
			"end": 0,
			"start_loop": 0,
			"end_loop": 0,
			"sample_rate": 0,
			"original_key": 0,
			"pitch_correction": 0,
			"sample_link": 0,
			"sample_type": 0,
		}
	
		shdr.name = chunk.stream.get_string( 20 )
		shdr.start = chunk.stream.get_u32( )
		shdr.end = chunk.stream.get_u32( )
		shdr.start_loop = chunk.stream.get_u32( )
		shdr.end_loop = chunk.stream.get_u32( )
		shdr.sample_rate = chunk.stream.get_u32( )
		shdr.original_key = chunk.stream.get_u8( )
		shdr.pitch_correction = chunk.stream.get_8( )
		shdr.sample_link = chunk.stream.get_u16( )
		shdr.sample_type = chunk.stream.get_u16( )
		shdrs.append( shdr )

	return shdrs
