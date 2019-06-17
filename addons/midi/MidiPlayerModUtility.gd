"""
	MOD converter for Godot MIDI Player by Yui Kinomoto @arlez80
"""

"""
	MODをGodot MIDI Player 用に変換
	@param	mod	MOD.gdで読み込んだMODファイル
	@return	{ smf: midiデータ, bank: サウンドバンク }
"""
func convert_mod_to_gmp( mod ):
	var smf = self._convert_pattern_data( mod.patterns )
	var bank = self._convert_sample_data( mod.samples )
	return { smf: smf, bank: bank }

"""
	MODのパターンデータをSMFに変換
	@param	patterns	MODパターン
	@return	SMF.gdで吐く構造体と同じもの
"""
func _convert_pattern_data( patterns ):
	var events = []

	# とりあえず構築する
	for pattern in patterns:
		var sample_number = -1
		var time = 0
		for event in pattern:
			if event.sample_number != sample_number:
				sample_number = event.sample_number
			events.append({
				"time": time,
			})
			time += 1

	# ソートして絶対時間順にならべる

	return {
		"format_type": 0,
		"track_count": patterns.length,
		"timebase": 16,
		"tracks": [{
			"track_number": 0,
			"events": events
		}],
	}
