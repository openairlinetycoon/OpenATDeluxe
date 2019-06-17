"""
	MOD reader by Yui Kinomoto @arlez80
"""

"""
	ファイルから読み込み
	@param	path	File path
	@return	smf
"""
func read_file( path:String ):
	var f:File = File.new( )

	if not f.file_exists( path ):
		print( "file %s is not found" % path )
		breakpoint

	f.open( path, f.READ )
	var stream:StreamPeerBuffer = StreamPeerBuffer.new( )
	stream.set_data_array( f.get_buffer( f.get_len( ) ) )
	stream.big_endian = true
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
	stream.big_endian = true
	return self._read( stream )

"""
	読み込み
	@param	stream
	@return	smf
"""
func _read( stream:StreamPeerBuffer ):
	var name:String = self._read_string( stream, 20 )
	var samples = self._read_sample_informations( stream )
	var song_length:int = stream.get_u8( )
	var unknown_number:int = stream.get_u8( )
	var song_positions = stream.get_partial_data( 128 )[1]
	var max_song_position:int = 0
	for sp in song_positions:
		if max_song_position < sp:
			max_song_position = sp

	var magic:String = self._read_string( stream, 4 )
	var channel_count:int = 4
	match magic:
		"6CHN":
			channel_count = 6
		"FLT8", "8CHN", "CD81", "OKTA":
			channel_count = 8
		"16CN":
			channel_count = 16
		"32CN":
			channel_count = 32
		_:
			# print( "Unknown magic" )
			# breakpoint
			pass

	var patterns = self._read_patterns( stream, max_song_position, channel_count )
	self._read_sample_data( stream, samples )

	return {
		"name": name,
		"song_length": song_length,
		"unknown_number": unknown_number,
		"song_positions": song_positions,
		"magic": magic,

		"patterns": patterns,
		"samples": samples,
	}

"""
	サンプルのデータを読み込む
"""
func _read_sample_informations( stream:StreamPeerBuffer ):
	var samples = []

	for i in range( 0, 31 ):
		var sample = {}
		sample.name = self._read_string( stream, 22 )
		sample.length = stream.get_u16( ) * 2
		sample.fine_tune = stream.get_u8( ) & 0x0F
		if 0x08 < sample.fine_tune:
			sample.fine_tune = 0x10 - sample.fine_tune
		sample.volume = stream.get_u8( )
		sample.loop_start = stream.get_u16( ) * 2
		sample.loop_length = stream.get_u16( ) * 2

		samples.append( sample )

	return samples

"""
	パターンを読み込む
"""
func _read_patterns( stream:StreamPeerBuffer, max_position:int, channels:int ):
	var patterns = []
	for position in range( 0, max_position ):
		var pattern = []
		for i in range( 0, 64 ):
			var line = []
			for ch in range( 0, channels ):
				var v1:int = stream.get_u16( )
				var v2:int = stream.get_u16( )
				var sample_number:int = ( ( v1 >> 8 ) & 0xF0 ) | ( ( v2 >> 12 ) & 0x0F )  
				var key_number:int = v1 & 0x0FFF
				var effect_command:int = v2 & 0x0FFF
				line.append({
					"sample_number": sample_number,
					"key_number": key_number,
					"effect_command": effect_command,
				})
			pattern.append( line )
		patterns.append( pattern )

"""
	波形データ読み込む
"""
func _read_sample_data( stream:StreamPeerBuffer, samples ):
	for sample in samples:
		sample.data = stream.get_partial_data( sample.length )[1]

"""
	文字列の読み込み
	@param	stream	Stream
	@param	size	string size
	@return string
"""
func _read_string( stream:StreamPeerBuffer, size:int ):
	return stream.get_partial_data( size )[1].get_string_from_ascii( )
