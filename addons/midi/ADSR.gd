extends AudioStreamPlayer

"""
	AudioStreamPlayer with ADSR
"""

# リリース中？
var releasing:bool = false
# リリース要求
var request_release:bool = false
# 楽器情報
var instrument = null
# 合成情報
var velocity:int = 0
var pitch_bend:float = 0.0
var pitch_bend_sensitivity:float = 12.0
var modulation:float = 0.0
var modulation_sensitivity:float = 0.5
var mix_rate:int = 0
# ADSRタイマー
var timer:float = 0.0
# 使用時間
var using_timer:float = 0.0

# 現在のADSRボリューム
var current_volume_db:float = 0.0
# 発音音量
var note_volume_db:float = 0.0
# 最大音量
var maximum_volume_db:float = -8.0
# 自動リリースモード？
var auto_release_mode:bool = false
# var pan:float = 0.5

# ADSステート
var ads_state = [
	{ "time": 0.0, "volume_db": 0.0 },
	{ "time": 0.2, "volume_db": -144.0 },
	# { "time": 0.2, "jump_to": 0.0 },	# not implemented
]
# Rステート
var release_state = [
	{ "time": 0.0, "volume_db": 0.0 },
	{ "time": 0.01, "volume_db": -144.0 },
	# { "time": 0.2, "jump_to": 0.0 },	# not implemented
]

func _ready( ):
	self.stop( )

func set_instrument( instrument ):
	self.instrument = instrument
	self.mix_rate = instrument.mix_rate
	self.stream = instrument.stream.duplicate( )
	self.ads_state = instrument.ads_state
	self.release_state = instrument.release_state

func play( from_position:float = 0.0 ):
	self.releasing = false
	self.request_release = false
	self.timer = 0.0
	self.using_timer = 0.0
	self.current_volume_db = self.ads_state[0].volume_db
	self.stream.mix_rate = self.mix_rate
	.play( from_position )
	self._update_volume( )

func start_release( ):
	self.request_release = true

func _update_adsr( delta:float ):
	if not self.playing:
		return

	self.timer += delta
	self.using_timer += delta
	# self.transform.origin.x = self.pan * self.get_viewport( ).size.x

	# ADSR選択
	var use_state = null
	if self.releasing:
		use_state = self.release_state
	else:
		use_state = self.ads_state

	var all_states:int = use_state.size( )
	var last_state:int = all_states - 1
	if use_state[last_state].time <= self.timer:
		self.current_volume_db = use_state[last_state].volume_db
		if self.releasing: self.stop( )
		if self.auto_release_mode: self.request_release = true
	else:
		for state_number in range( 1, all_states ):
			var state = use_state[state_number]
			if self.timer < state.time:
				var pre_state = use_state[state_number-1]
				var s:float = ( state.time - self.timer ) / ( state.time - pre_state.time )
				var t:float = 1.0 - s
				self.current_volume_db = pre_state.volume_db * s + state.volume_db * t
				break

	var pitch_bend = self.pitch_bend * self.pitch_bend_sensitivity / 12.0
	var modulation = sin( self.using_timer * 32.0 ) * ( self.modulation * self.modulation_sensitivity / 12.0 )
	self.pitch_scale = pow( 2, modulation + pitch_bend )

	self._update_volume( )

	if self.request_release and not self.releasing:
		self.releasing = true
		self.current_volume_db = self.release_state[0].volume_db
		self.timer = 0.0

func _update_volume( ):
	self.volume_db = self.current_volume_db + self.note_volume_db + self.maximum_volume_db

func change_channel_volume( base_volume_db:float, channel ):
	self.note_volume_db = linear2db( float( channel.volume * channel.expression ) * ( float( self.velocity ) / 127.0 ) )
	self.maximum_volume_db = base_volume_db
