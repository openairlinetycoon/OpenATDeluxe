"""
	SMF reader/writer by Yui Kinomoto @arlez80
"""

# -----------------------------------------------------------------------------
# 定数

# Control Numbers
const control_number_bank_select_msb = 0x00
const control_number_modulation = 0x01
const control_number_breath_controller = 0x02
const control_number_foot_controller = 0x04
const control_number_portamento_time = 0x05
const control_number_data_entry_msb = 0x06
const control_number_volume = 0x07
const control_number_balance = 0x08
const control_number_pan = 0x0A
const control_number_expression = 0x0B

const control_number_bank_select_lsb = 0x20
const control_number_modulation_lsb = 0x21
const control_number_breath_controller_lsb = 0x22
const control_number_foot_controller_lsb = 0x24
const control_number_portamento_time_lsb = 0x25
const control_number_data_entry_lsb = 0x26
const control_number_channel_volume_lsb = 0x27
const control_number_calance_lsb = 0x28
const control_number_pan_lsb = 0x2A
const control_number_expression_lsb = 0x2B
const control_number_effect_control1 = 0x2C
const control_number_effect_control2 = 0x2D

const control_number_hold = 0x40		# Sustain Pedal
const control_number_portament = 0x41
const control_number_sostenuto = 0x42
const control_number_soft_pedal = 0x43
const control_number_legato_foot_switch = 0x44
const control_number_freeze = 0x45
const control_number_sound_variation = 0x46
const control_number_timbre = 0x47
const control_number_release_time = 0x48
const control_number_attack_time = 0x49
const control_number_brightness = 0x4A
const control_number_vibrato_rate = 0x4B
const control_number_vibrato_depth = 0x4C
const control_number_vibrato_delay = 0x4D

const control_number_source_note = 0x54

const control_number_high_res_velovity_prefix = 0x58

const control_number_reverb_send_level = 0x5B	# Effect 1
const control_number_tremolo_depth = 0x5C		# Effect 2
const control_number_chorus_send_level = 0x5D	# Effect 3
const control_number_celeste_depth = 0x5E		# Effect 4
const control_number_phaser_depth = 0x5F		# Effect 5

const control_number_data_increment = 0x60
const control_number_data_decrement = 0x61
const control_number_nrpn_lsb = 0x62
const control_number_nrpn_msb = 0x63
const control_number_rpn_lsb = 0x64
const control_number_rpn_msb = 0x65
const control_number_tkool_loop_point = 0x6F	# CC111

# RPN Control Numbers: 0x00
const rpn_control_number_pitch_bend_sensitivity = 0x00
const rpn_control_number_channel_fine_tune = 0x01
const rpn_control_number_channel_cource_tune = 0x02
const rpn_control_number_tune_program_change = 0x03
const rpn_control_number_tune_bank_select = 0x04
# RPN Control Numbers: 0x3D
const rpn_control_number_3D_azimuth_angle = 0x00
const rpn_control_number_3D_elevation_angle = 0x01
const rpn_control_number_3D_gain = 0x02
const rpn_control_number_3D_distance_ratio = 0x03
const rpn_control_number_3D_maximum_distance = 0x04
const rpn_control_number_3D_gain_at_maximum_distance = 0x05
const rpn_control_number_3D_referance_distance_raito = 0x06
const rpn_control_number_3D_pan_spread_angle = 0x07
const rpn_control_number_3D_roll_angle = 0x08

# Manufacture ID
const manufacture_id_universal_nopn_realtime_sys_ex = 0x7E
const manufacture_id_universal_realtime_sys_ex = 0x7F
const manufacture_id_kawai_musical_instruments_mfg_co_ltd = 0x40
const manufacture_id_roland_corporation = 0x41
const manufacture_id_korg_inc = 0x42
const manufacture_id_yamaha_corporation = 0x43
const manufacture_id_casio_computer_co_ltd = 0x44
const manufacture_id_kamiya_studio_co_ltd = 0x46
const manufacture_id_akai_electric_co_ltd = 0x47

# 7F

# Enums
enum MIDIEventType {
	note_off,					# 8*
	note_on,					# 9*
	polyphonic_key_pressure,	# A*
	control_change,				# B*
	program_change,				# C*
	channel_pressure,			# D*
	pitch_bend,					# E*
	system_event,				# F*
}

enum MIDISystemEventType {
	sys_ex,					
	divided_sys_ex,			

	sequence_number,		# 00
	text_event,				# 01
	copyright,				# 02
	track_name,				# 03
	instrument_name,		# 04
	lyric,					# 05
	marker,					# 06
	cue_point,				# 07

	midi_channel_prefix,	# 20
	midi_port_prefix,		# 21	not standard
	end_of_track,			# 2F

	set_tempo,				# 51

	smpte_offset,			# 54

	beat,					# 58
	key,					# 59

	unknown,
}

# -----------------------------------------------------------------------------
# 読み込み : Reader

var last_event_type:int = 0

"""
	ファイルから読み込み
	@param	path	File path
	@return	smf or null(read error)
"""
func read_file( path:String ):
	var f = File.new( )

	if f.open( path, f.READ ) != OK:
		push_error( "cant read file %s" % path )
		breakpoint
	var stream:StreamPeerBuffer = StreamPeerBuffer.new( )
	stream.set_data_array( f.get_buffer( f.get_len( ) ) )
	stream.big_endian = true
	f.close( )

	return self._read( stream )

"""
	配列から読み込み
	@param	data	PoolByteArray
	@return	smf or null(read error)
"""
func read_data( data:PoolByteArray ):
	var stream:StreamPeerBuffer = StreamPeerBuffer.new( )
	stream.set_data_array( data )
	stream.big_endian = true
	return self._read( stream )

"""
	読み込み
	@param	input
	@return	smf
"""
func _read( input:StreamPeerBuffer ):
	var header = self._read_chunk_data( input )
	if header.id != "MThd" and header.size != 6:
		print( "expected MThd header" )
		return null

	var format_type:int = header.stream.get_u16( )
	var track_count:int = header.stream.get_u16( )
	var timebase:int = header.stream.get_u16( )

	var tracks = []
	for i in range( 0, track_count ):
		var track = self._read_track( input, i )
		if track == null:
			return null
		tracks.append( track )

	return {
		"format_type": format_type,
		"track_count": track_count,
		"timebase": timebase,
		"tracks": tracks,
	}

"""
	トラックの読み込み
	@param	input
	@param	track_number	トラックナンバー
	@return	track data or null(read error)
"""
func _read_track( input:StreamPeerBuffer, track_number:int ):
	var track_chunk = self._read_chunk_data( input )
	if track_chunk.id != "MTrk":
		print( "Unknown chunk: " + track_chunk.id )
		return null

	var stream:StreamPeerBuffer = track_chunk.stream
	var time:int = 0
	var events = []

	while 0 < stream.get_available_bytes( ):
		var delta_time:int = self._read_variable_int( stream )
		time += delta_time
		var event_type_byte:int = stream.get_u8( )

		var event
		if self._is_system_event( event_type_byte ):
			var args = self._read_system_event( stream, event_type_byte )
			if args == null: return null
			event = {
				"type": MIDIEventType.system_event,
				"args": args
			}
		else:
			event = self._read_event( stream, event_type_byte )
			if event == null: return null

			# running status
			if ( event_type_byte & 0x80 ) == 0:
				event_type_byte = self.last_event_type

		events.append({
			"time": time,
			"channel_number": event_type_byte & 0x0f,
			"event": event,
		})

	return {
		"track_number": track_number,
		"events": events,
	}

"""
	システムイベントか否かを返す
	@param	b	event type
	@return	システムイベントならtrueを返す
"""
func _is_system_event( b:int ):
	return ( b & 0xf0 ) == 0xf0

"""
	システムイベントの読み込み
"""
func _read_system_event( stream:StreamPeerBuffer, event_type_byte:int ):
	if event_type_byte == 0xff:
		var meta_type:int = stream.get_u8( )
		var size:int = self._read_variable_int( stream )

		match meta_type:
			0x00:
				return { "type": MIDISystemEventType.sequence_number, "number": stream.get_u16( ) }
			0x01:
				return { "type": MIDISystemEventType.text_event, "text": self._read_string( stream, size ) }
			0x02:
				return { "type": MIDISystemEventType.copyright, "text": self._read_string( stream, size ) }
			0x03:
				return { "type": MIDISystemEventType.track_name, "text": self._read_string( stream, size ) }
			0x04:
				return { "type": MIDISystemEventType.instrument_name, "text": self._read_string( stream, size ) }
			0x05:
				return { "type": MIDISystemEventType.lyric, "text": self._read_string( stream, size ) }
			0x06:
				return { "type": MIDISystemEventType.marker, "text": self._read_string( stream, size ) }
			0x07:
				return { "type": MIDISystemEventType.cue_point, "text": self._read_string( stream, size ) }
			0x20:
				if size != 1:
					print( "MIDI Channel Prefix length is not 1 byte" )
					return null
				return { "type": MIDISystemEventType.midi_channel_prefix, "channel": stream.get_u8( ) }
			0x21:
				if size != 1:
					print( "MIDI Port Prefix length is not 1 byte" )
					return null
				return { "type": MIDISystemEventType.midi_port_prefix, "port": stream.get_u8( ) }
			0x2F:
				if size != 0:
					print( "End of track with unknown data" )
					return null
				return { "type": MIDISystemEventType.end_of_track }
			0x51:
				if size != 3:
					print( "Tempo length is not 3 bytes" )
					return null
				# beat per microseconds
				var bpm:int = stream.get_u8( ) << 16
				bpm |= stream.get_u8( ) << 8
				bpm |= stream.get_u8( )
				return { "type": MIDISystemEventType.set_tempo, "bpm": bpm }
			0x54:
				if size != 5:
					print( "SMPTE length is not 5 bytes" )
					return null
				var hr:int = stream.get_u8( )
				var mm:int = stream.get_u8( )
				var se:int = stream.get_u8( )
				var fr:int = stream.get_u8( )
				var ff:int = stream.get_u8( )
				return {
					"type": MIDISystemEventType.smpte_offset,
					"hr": hr,
					"mm": mm,
					"se": se,
					"fr": fr,
					"ff": ff,
				}
			0x58:
				if size != 4:
					print( "Beat length is not 4 bytes" )
					return null
				var numerator:int = stream.get_u8( )
				var denominator:int = stream.get_u8( )
				var clock:int = stream.get_u8( )
				var beat32:int = stream.get_u8( )
				return {
					"type": MIDISystemEventType.beat,
					"numerator": numerator,
					"denominator": denominator,
					"clock": clock,
					"beat32": beat32,
				}
			0x59:
				if size != 2:
					print( "Key length is not 2 bytes" )
					return null
				var sf:int = stream.get_u8( )
				var minor:int = stream.get_u8( ) == 1
				return {
					"type": MIDISystemEventType.key,
					"sf": sf,
					"minor": minor,
				}
			_:
				return {
					"type": MIDISystemEventType.unknown,
					"meta_type": meta_type,
					"data": stream.get_partial_data( size )[1],
				}
	elif event_type_byte == 0xf0:
		var size:int = self._read_variable_int( stream )
		return {
			"type": MIDISystemEventType.sys_ex,
			"manifacture_id": stream.get_u8( ),
			"data": stream.get_partial_data( size - 1 )[1],
		}
	elif event_type_byte == 0xf7:
		var size:int = self._read_variable_int( stream )
		return {
			"type": MIDISystemEventType.divided_sys_ex,
			"manifacture_id": stream.get_u8( ),
			"data": stream.get_partial_data( size - 1 )[1],
		}

	print( "Unknown system event type: %x" % event_type_byte )
	return null

"""
	通常のイベント読み込み
	@param	stream
	@param	event_type_byte
	@return	MIDIEvent
"""
func _read_event( stream:StreamPeerBuffer, event_type_byte:int ):
	var param:int = 0

	if ( event_type_byte & 0x80 ) == 0:
		# running status
		param = event_type_byte
		event_type_byte = self.last_event_type
	else:
		param = stream.get_u8( )
		self.last_event_type = event_type_byte

	var event_type:int = event_type_byte & 0xf0

	match event_type:
		0x80:
			return {
				"type": MIDIEventType.note_off,
				"note": param,
				"velocity": stream.get_u8( ),
			}
		0x90:
			var velocity:int = stream.get_u8( )
			if velocity == 0:
				# velocity0のnote_onはnote_off扱いにする
				return {
					"type": MIDIEventType.note_off,
					"note": param,
					"velocity": velocity,
				}
			else:
				return {
					"type": MIDIEventType.note_on,
					"note": param,
					"velocity": velocity,
				}
		0xA0:
			return {
				"type": MIDIEventType.polyphonic_key_pressure,
				"note": param,
				"value": stream.get_u8( ),
			}
		0xB0:
			return {
				"type": MIDIEventType.control_change,
				"number": param,
				"value": stream.get_u8( ),
			}
		0xC0:
			return {
				"type": MIDIEventType.program_change,
				"number": param,
			}
		0xD0:
			return {
				"type": MIDIEventType.channel_pressure,
				"value": param,
			}
		0xE0:
			return {
				"type": MIDIEventType.pitch_bend,
				"value": param | ( stream.get_u8( ) << 7 ),
			}

	print( "unknown event type: %d" % event_type_byte )
	return null

"""
	可変長数値の読み込み
	@param	stream
	@return	数値
"""
func _read_variable_int( stream:StreamPeerBuffer ):
	var result:int = 0

	while true:
		var c:int = stream.get_u8( )
		if ( c & 0x80 ) != 0:
			result |= c & 0x7f
			result <<= 7
		else:
			result |= c
			break

	return result

"""
	チャンクデータの読み込み
	@param	stream	Stream
	@return	chunk data
"""
func _read_chunk_data( stream:StreamPeerBuffer ):
	var id:String = self._read_string( stream, 4 )
	var size:int = stream.get_32( )
	var new_stream:StreamPeerBuffer = StreamPeerBuffer.new( )
	new_stream.set_data_array( stream.get_partial_data( size )[1] )
	new_stream.big_endian = true

	return {
		"id": id,
		"size": size,
		"stream": new_stream
	}

"""
	文字列の読み込み
	@param	stream	Stream
	@param	size	string size
	@return string
"""
func _read_string( stream:StreamPeerBuffer, size:int ):
	return stream.get_partial_data( size )[1].get_string_from_ascii( )

# -----------------------------------------------------------------------------
# 書き込み: Writer

"""
	書き込む
	@param	smf	SMF structure
	@return	PoolByteArray
"""
func write( smf ):
	var stream:StreamPeerBuffer = StreamPeerBuffer.new( )
	stream.big_endian = true
	
	stream.put_utf8_string( "MThd" )
	stream.put_u32( 6 )
	stream.put_u16( smf.format_type )
	stream.put_u16( len( smf.tracks ) )
	stream.put_u16( smf.timebase )

	for t in smf.tracks:
		self._write_track( stream, t )

	return stream.get_partial_data( stream.get_available_bytes( ) )[1]

"""
	トラックデータソート用
"""
class TrackEventSorter:
	static func sort( a, b ):
		if a.time < b.time:
			return true
		return false

"""
	可変長数字を書き込む
	@param	stream
	@param	i
"""
func _write_variable_int( stream:StreamPeerBuffer, i:int ):
	while true:
		var v = i & 0x7f
		i >>= 7
		if i != 0:
			stream.put_u8( v | 0x80 )
		else:
			stream.put_u8( v )
			break

"""
	トラックデータを書き込む
	@param	stream
	@param	track
"""
func _write_track( stream:StreamPeerBuffer, track ):
	var events = track.events.duplicate( )
	events.sort_custom( TrackEventSorter, "sort" )

	var buf:StreamPeerBuffer = StreamPeerBuffer.new( )
	buf.big_endian = true
	var time:int = 0

	for e in events:
		self._write_variable_int( buf, e.time - time )
		time = e.time
		match e.type:
			MIDIEventType.note_off:
				buf.put_u8( 0x80 | e.channel_number )
				buf.put_u8( e.note )
				buf.put_u8( e.velocity )
			MIDIEventType.note_on:
				buf.put_u8( 0x90 | e.channel_number )
				buf.put_u8( e.note )
				buf.put_u8( e.velocity )
			MIDIEventType.polyphonic_key_pressure:
				buf.put_u8( 0xA0 | e.channel_number )
				buf.put_u8( e.note )
				buf.put_u8( e.value )
			MIDIEventType.control_change:
				buf.put_u8( 0xB0 | e.channel_number )
				buf.put_u8( e.number )
				buf.put_u8( e.value )
			MIDIEventType.program_change:
				buf.put_u8( 0xC0 | e.channel_number )
				buf.put_u8( e.number )
			MIDIEventType.channel_pressure:
				buf.put_u8( 0xD0 | e.channel_number )
				buf.put_u8( e.value )
			MIDIEventType.pitch_bend:
				buf.put_u8( 0xE0 | e.channel_number )
				buf.put_u8( e.value & 0x7f )
				buf.put_u8( ( e.value >> 7 ) & 0x7f )
			MIDIEventType.system_event:
				self._write_system_event( buf, e )

	var track_size:int = buf.get_available_bytes( )
	stream.put_utf8_string( "MTrk" )
	stream.put_u32( track_size )
	if stream.put_data( buf.get_partial_data( track_size )[1] ) != OK:
		push_error( "cant write tracksize" )
		breakpoint

"""
	システムイベント書き込み
	@param	stream
	@param	event
"""
func _write_system_event( stream:StreamPeerBuffer, event ):
	match event.type:
		MIDISystemEventType.sys_ex:
			stream.put_u8( 0xF0 )
			stream.put_u8( event.manifacture_id )
			self._write_variable_int( stream, len( event.data ) )
			if stream.put_data( event.data ) != OK:
				push_error( "cant write event data" )
				breakpoint
		MIDISystemEventType.divided_sys_ex:
			stream.put_u8( 0xF7 )
			stream.put_u8( event.manifacture_id )
			self._write_variable_int( stream, len( event.data ) )
			if stream.put_data( event.data ) != OK:
				push_error( "cant write event data" )
				breakpoint

		MIDISystemEventType.text_event:
			stream.put_u8( 0xFF )
			stream.put_u8( 0x01 )
			self._write_variable_int( stream, len( event.text ) )
			if stream.put_data( event.text.to_ascii( ) ) != OK:
				push_error( "cant write text event" )
				breakpoint
		MIDISystemEventType.copyright:
			stream.put_u8( 0xFF )
			stream.put_u8( 0x02 )
			self._write_variable_int( stream, len( event.text ) )
			if stream.put_data( event.text.to_ascii( ) ) != OK:
				push_error( "cant write copyright text" )
				breakpoint
		MIDISystemEventType.track_name:
			stream.put_u8( 0xFF )
			stream.put_u8( 0x03 )
			self._write_variable_int( stream, len( event.text ) )
			if stream.put_data( event.text.to_ascii( ) ) != OK:
				push_error( "cant write track name text" )
				breakpoint
		MIDISystemEventType.instrument_name:
			stream.put_u8( 0xFF )
			stream.put_u8( 0x04 )
			self._write_variable_int( stream, len( event.text ) )
			if stream.put_data( event.text.to_ascii( ) ) != OK:
				push_error( "cant write instrument name" )
				breakpoint
		MIDISystemEventType.lyric:
			stream.put_u8( 0xFF )
			stream.put_u8( 0x05 )
			self._write_variable_int( stream, len( event.text ) )
			if stream.put_data( event.text.to_ascii( ) ) != OK:
				push_error( "cant write lyric text" )
				breakpoint
		MIDISystemEventType.marker:
			stream.put_u8( 0xFF )
			stream.put_u8( 0x06 )
			self._write_variable_int( stream, len( event.text ) )
			if stream.put_data( event.text.to_ascii( ) ) != OK:
				push_error( "cant write marker text" )
				breakpoint
		MIDISystemEventType.cue_point:
			stream.put_u8( 0xFF )
			stream.put_u8( 0x07 )
			self._write_variable_int( stream, len( event.text ) )
			if stream.put_data( event.text.to_ascii( ) )!= OK:
				push_error( "cant write cue point" )
				breakpoint

		MIDISystemEventType.midi_channel_prefix:
			stream.put_u8( 0xFF )
			stream.put_u8( 0x20 )
			stream.put_u8( 0x01 )
			stream.put_u8( event.prefix )
		MIDISystemEventType.midi_channel_port:
			stream.put_u8( 0xFF )
			stream.put_u8( 0x21 )
			stream.put_u8( 0x01 )
			stream.put_u8( event.prefix )
		MIDISystemEventType.end_of_track:
			stream.put_u8( 0xFF )
			stream.put_u8( 0x2F )
			stream.put_u8( 0x00 )
		MIDISystemEventType.set_tempo:
			stream.put_u8( 0xFF )
			stream.put_u8( 0x51 )
			stream.put_u8( 0x03 )
			stream.put_u8( ( event.bpm >> 16 ) & 0xFF )
			stream.put_u8( ( event.bpm >> 8 ) & 0xFF )
			stream.put_u8( event.bpm & 0xFF )
		MIDISystemEventType.smpte_offset:
			stream.put_u8( 0xFF )
			stream.put_u8( 0x54 )
			stream.put_u8( 0x05 )
			stream.put_u8( event.hr )
			stream.put_u8( event.mm )
			stream.put_u8( event.se )
			stream.put_u8( event.fr )
			stream.put_u8( event.ff )
		MIDISystemEventType.beat:
			stream.put_u8( 0xFF )
			stream.put_u8( 0x58 )
			stream.put_u8( 0x04 )
			stream.put_u8( event.numerator )
			stream.put_u8( event.denominator )
			stream.put_u8( event.clock )
			stream.put_u8( event.beat32 )
		MIDISystemEventType.key:
			stream.put_u8( 0xFF )
			stream.put_u8( 0x59 )
			stream.put_u8( 0x02 )
			stream.put_u8( event.sf )
			stream.put_u8( 1 if event.minor else 0 )
		MIDISystemEventType.unknown:
			stream.put_u8( 0xFF )
			stream.put_u8( event.meta_type )
			stream.put_u8( len( event.data ) )
			if stream.put_data( event.data ) != OK:
				push_error( "cant write event data" )
				breakpoint
		_:
			push_error( "not implemented! %d" % event.type )
			breakpoint
