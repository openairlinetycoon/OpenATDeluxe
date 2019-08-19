using Godot;
using System;

public static class OneShotAction {
	public static void CreateOneShot(Action baseAction, Action actionToAdd) {
		Action oneShot = null;
		oneShot = () => {
			baseAction -= oneShot;

			actionToAdd?.Invoke();
		};


		baseAction += oneShot;
	}
}
