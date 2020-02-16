using System;
using System.Linq;
using System.Collections.Generic;
using Godot;

static class InteractionLayerManager {
	static InteractionLayerManager() {
		ResetLayers();
	}

	public static int highestLayer;
	public static List<int> disabledLayers;

	public static int GetCurrentLayer(Node me) {
		Node parent;
		
		while ((parent = me.GetParentOrNull<Node>()) != null) {
			if (parent is IInteractionLayer layer) {
				return layer.Layer;
			}
		}

		return 0;
	}

	public static int AddLayer() {
		return ++highestLayer;
	}

	public static void ResetLayers() {
		disabledLayers = new List<int>();
		highestLayer = Enum.GetValues(typeof(BaseLayer)).Length;
	}

	public static void DisableLayers(int[] layersToDisable) {
		disabledLayers.AddRange(layersToDisable);

		disabledLayers = disabledLayers.Distinct().ToList();
	}

	/// <summary>
	/// Disables all layers but the "currentLayer"
	/// </summary>
	/// <param name="currentLayer">The only active layer</param>
	public static void DisableAllLayersButOne(int currentLayer) {
		if (currentLayer > highestLayer) {
			throw new ArgumentOutOfRangeException($"The given layer ({currentLayer}) is higher than the highest layer ({highestLayer})");
		}

		DisableAllLayers();

		disabledLayers.Remove(currentLayer);
	}
	public static void DisableAllLayersButOne(BaseLayer currentLayer) {
		DisableAllLayersButOne((int) currentLayer);
	}

	public static void DisableAllLayers() {
		disabledLayers.Clear();
		disabledLayers.AddRange(Enumerable.Range(0, highestLayer).ToList());
	}
	public static void EnableAllLayers() {
		disabledLayers.Clear();
	}
	public static void EnableLayer(int layer) {
		disabledLayers.Remove(layer);
	}

	public static bool IsLayerDisabled(int layer) {
		return disabledLayers.Contains(layer);
	}
	
	public static void EnableLayer(BaseLayer layer) {
		disabledLayers.Remove((int)layer);
	}

	public static bool IsLayerDisabled(BaseLayer layer) {
		return disabledLayers.Contains((int)layer);
	}
	
	public static void EnableLayer(IInteractionLayer layer) {
		disabledLayers.Remove(layer.Layer);
	}

	public static bool IsLayerDisabled(IInteractionLayer layer) {
		return disabledLayers.Contains(layer.Layer);
	}


}

public enum BaseLayer : int {
	Default = 0,
	MouseAreaRoom,
	MouseArea,
	Elements,
}