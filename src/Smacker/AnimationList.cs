using Godot;
using System;
using System.Collections.Generic;

public class AnimationList : List<SmkAnimation> {
	public static Action OnCancelTrigger;

	public Vector2 basePosition;

	public SmkAnimation current;
	public MouseArea mouseArea;

	public MouseArea CreateMouseArea(Node parent) {
		mouseArea = new MouseArea();
		CollisionShape2D area = new CollisionShape2D();
		area.Shape = new RectangleShape2D();
		area.ZIndex = 1;
		mouseArea.AddChild(area);
		parent.AddChild(mouseArea);

		return mouseArea;
	}

	public void Play(int index) {
		StopAll();

		current = this[index];

		if (mouseArea != null) {
			int width = current.GetWidth(), height = current.GetHeight();
			mouseArea.area.Scale = new Vector2(width / 2, height / 2) / 10f;
			mouseArea.area.Position = new Vector2(width / 2, height / 2) + basePosition;
		}
		current.Play();
	}

	public new void Add(SmkAnimation item) {
		base.Add(item);

		item.Position += basePosition;

		if (item.goal.finish != null || item.goal.finishID != -1)
			item.OnAnimationFinish += (item.goal.finish != null ? (Action)(() => Play(item.goal.finish)) : (() => Play(item.goal.finishID)));

	}

	public void Play(SmkAnimation animation) {
		StopAll();

		current = Find((toFind) => toFind == animation);

		if (mouseArea != null) {
			int width = current.GetWidth(), height = current.GetHeight();
			mouseArea.area.Scale = new Vector2(width / 2, height / 2) / 10f;
			mouseArea.area.Position = new Vector2(width / 2, height / 2) + basePosition;
		}

		current.Play();
	}

	public void StopAll() {
		foreach (SmkAnimation p in this) {
			p.Hide();
		}
	}

	public void Stop(int index) {
		foreach (SmkAnimation p in this) {
			p.Hide();
		}

		current = this[index];
		current.Visible = true;
		current.isPlaying = false;
	}

	public void Cancel() {
		SmkAnimation anim = current?.goal?.cancel;
		if (current.goal.cancelID != -1)
			Play(current.goal.cancelID);

		if (anim != null)
			Play(anim);
	}

	public void Trigger() {
		SmkAnimation anim = current.goal.trigger;
		if (current.goal.triggerID != -1)
			Play(current.goal.triggerID);

		if (anim != null)
			Play(anim);
	}

	public void ProcessTrigger() {
		if (current?.goal?.onTrigger?.Invoke(current.goal) == true)
			Trigger();
	}
}


public class AnimationGoal {
	/// <summary>
	/// When a animation is done, go to this animation:
	/// When null -> loop until other criterea are set.      
	/// </summary>
	public SmkAnimation finish;
	public int finishID;
	/// <summary>
	/// When a cancel message is recieved, go to this animation
	/// </summary>
	public SmkAnimation cancel;
	public int cancelID;
	/// <summary>
	/// When the current animation gets triggered by a call to smkPlayer.Trigger(), go to this animation
	/// </summary>
	public SmkAnimation trigger;
	public int triggerID;

	public Func<AnimationGoal, bool> onTrigger;

	public AnimationGoal(SmkAnimation finish = null, SmkAnimation cancel = null, SmkAnimation trigger = null, Func<AnimationGoal, bool> onTrigger = null) {
		this.finish = finish;
		this.cancel = cancel;
		this.trigger = trigger;
		finishID = -1;
		cancelID = -1;
		triggerID = -1;

		this.onTrigger = onTrigger;
	}
	public AnimationGoal(int finish = -1, int cancel = -1, int trigger = -1, Func<AnimationGoal, bool> onTrigger = null) {
		this.finishID = finish;
		this.cancelID = cancel;
		this.triggerID = trigger;
		this.finish = null;
		this.cancel = null;
		this.trigger = null;

		this.onTrigger = onTrigger;
	}
	public AnimationGoal(Func<AnimationGoal, bool> onTrigger) {
		finishID = -1;
		cancelID = -1;
		triggerID = -1;
		this.finish = null;
		this.cancel = null;
		this.trigger = null;

		this.onTrigger = onTrigger;
	}

	public AnimationGoal() {
		finishID = -1;
		cancelID = -1;
		triggerID = -1;
		this.finish = null;
		this.cancel = null;
		this.trigger = null;
	}
}