using Godot;
using System;
using System.Linq;

public class SmkAnimation : SmkPlayer {
	public SmkAnimation onFinish, onCancel, onTrigger;

	public SoundPlayer audio;

	public AnimationGoal goal;

	public static SmkAnimation CreateAnimation(Node parent, string name, Vector2 position = default(Vector2), AnimationGoal goal = default(AnimationGoal), SoundPlayer audio = null, string folder = "/video/") {
		SmkAnimation smkAnimation = new SmkAnimation();

		smkAnimation.fileName = folder + name;
		smkAnimation.Position = position;
		smkAnimation.audio = audio;

		smkAnimation.Centered = false;
		smkAnimation.UseParentMaterial = true;
		smkAnimation.isPlaying = false;
		smkAnimation.Visible = false;
		smkAnimation.audio = audio;

		if (audio != null) {
			audio.use8BitEncoding = true;
		}

		smkAnimation.goal = goal;
		if (goal == null) {
			smkAnimation.goal = new AnimationGoal();
		}

		smkAnimation.SetAudio();

		parent.AddChild(smkAnimation, true);

		return smkAnimation;
	}

	override public void _Ready() {
		base._Ready();

		if (audio == null) {
			//audio = (SoundPlayer)GetChildren();//((a) => a is SoundPlayer);
			//TODO: Utilize a pooling system, to prevent longer sounds from being cut off, when the animation finishes earlier
			SetAudio();
		}
	}

	private void SetAudio() {
		if (audio != null) {
			if (audio.GetParent() != this)
				AddChild(audio);

			void MakeSound() {
				audio.Play();
			}

			//First time
			OnPlay += MakeSound;

			if (goal?.finish != null || goal == null) {
				//When animation repeats
				OnAnimationFinish += MakeSound;
			}
		}
	}
}