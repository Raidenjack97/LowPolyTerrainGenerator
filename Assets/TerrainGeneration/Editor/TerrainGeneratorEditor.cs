﻿using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (TerrainGenerator))]
public class TerrainGeneratorEditor : Editor {
	
	private TerrainGenerator Target;

	private bool LeftMouseDown = false;
	private RaycastHit Hit;

	private System.DateTime Time;
	private float DeltaTime;

	private Vector2i Resolution;

	void Awake() {
		Target = (TerrainGenerator)target;
        if (Target.TerrainSystem != null)
		    Resolution = Target.TerrainSystem.Resolution;
	}

	void OnEnable() {
		Tools.hidden = true;
	}

	void OnDisable() {
		Tools.hidden = false;
	}

    void OnSceneGUI() {
		DeltaTime = Mathf.Min(0.01f, (float)(System.DateTime.Now-Time).Duration().TotalSeconds); //100Hz Baseline
		Time = System.DateTime.Now;
		if(Event.current.type == EventType.Layout) {
			HandleUtility.AddDefaultControl(0);
		}
		DrawCursor();
		HandleInteraction();
    }

	public override void OnInspectorGUI() {
		Undo.RecordObject(Target, Target.name);
		Inspect();
		if(GUI.changed) {
			EditorUtility.SetDirty(Target);
			Target.TerrainSystem.SetHeightMap(Target.TerrainSystem.CreateHeightMap(
				Target.Seed, Target.Scale, Target.Octaves, Target.Persistance, Target.Lacunarity, Target.FalloffStrength, Target.FalloffRamp, Target.FalloffRange, Target.Offset, Target.HeightMultiplier, Target.HeightCurve
			));
			Target.TerrainSystem.SetColorMap(Target.TerrainSystem.CreateColorMap());
		}
	}

	private void Inspect() {
		InspectWorld(Target.TerrainSystem);
		InspectTerrain();
		InspectTools();
		if(GUILayout.Button("Reset")) {
			Target.TerrainSystem.Reinitialise();
		}
	}

	private void InspectWorld(TerrainSystem terrainSystem) {
		if(terrainSystem == null) {
			return;
		}
		using(new EditorGUILayout.VerticalScope ("Button")) {
			GUI.backgroundColor = Color.white;
			EditorGUILayout.HelpBox("World", MessageType.None);

			terrainSystem.SetSize(EditorGUILayout.Vector2Field("Size", terrainSystem.Size));
			Vector2 resolution = EditorGUILayout.Vector2Field("Resolution", new Vector2(Resolution.x, Resolution.y));
			Resolution = new Vector2i((int)resolution.x, (int)resolution.y);
			if(Resolution.x != terrainSystem.Resolution.x || Resolution.y != terrainSystem.Resolution.y) {
				EditorGUILayout.HelpBox("Changing the resolution will reset the world.", MessageType.Warning);
				if(GUILayout.Button("Apply")) {
					terrainSystem.SetResolution(Resolution);
				}
			}
		}
	}

	private void InspectTerrain() {
		using(new EditorGUILayout.VerticalScope ("Button")) {
			GUI.backgroundColor = Color.white;
			EditorGUILayout.HelpBox("Terrain", MessageType.None);

			Target.Seed = EditorGUILayout.IntField("Seed", Target.Seed);
			Target.Scale = EditorGUILayout.FloatField("Scale", Target.Scale);
			Target.Octaves = EditorGUILayout.IntField("Octaves", Target.Octaves);
			Target.Persistance = EditorGUILayout.FloatField("Persistance", Target.Persistance);
			Target.Lacunarity = EditorGUILayout.FloatField("Lacunarity", Target.Lacunarity);
			Target.FalloffStrength = EditorGUILayout.FloatField("FalloffStrength", Target.FalloffStrength);
			Target.FalloffRamp = EditorGUILayout.FloatField("FalloffRamp", Target.FalloffRamp);
			Target.FalloffRange = EditorGUILayout.FloatField("FalloffRange", Target.FalloffRange);
			Target.Offset = EditorGUILayout.Vector2Field("Offset", Target.Offset);
			Target.HeightMultiplier = EditorGUILayout.FloatField("HeightMultiplier", Target.HeightMultiplier);
			Target.HeightCurve = EditorGUILayout.CurveField("HeightCurve", Target.HeightCurve);
            Target.TerrainSystem = (TerrainSystem)EditorGUILayout.ObjectField("TerrainSystem", Target.TerrainSystem, typeof(TerrainSystem), true, null);

            if (Target.TerrainSystem == null)
                return;

			using(new EditorGUILayout.VerticalScope ("Button")) {
				GUI.backgroundColor = Color.white;
				EditorGUILayout.HelpBox("Biomes", MessageType.None);
				
				Target.TerrainSystem.Interpolation = EditorGUILayout.Slider("Interpolation", Target.TerrainSystem.Interpolation, 0f, 1f);
				Target.TerrainSystem.FilterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", Target.TerrainSystem.FilterMode);
				for(int i=0; i<Target.TerrainSystem.Biomes.Length; i++) {
					Target.TerrainSystem.Biomes[i].Color = EditorGUILayout.ColorField(Target.TerrainSystem.Biomes[i].Color);
					float start = Target.TerrainSystem.Biomes[i].StartHeight;
					float end = Target.TerrainSystem.Biomes[i].EndHeight;
					EditorGUILayout.MinMaxSlider(ref start, ref end, 0f, 1f);
					Target.TerrainSystem.SetBiomeStartHeight(i, start);
					Target.TerrainSystem.SetBiomeEndHeight(i, end);
				}
				if(GUILayout.Button("Add Biome")) {
					System.Array.Resize(ref Target.TerrainSystem.Biomes, Target.TerrainSystem.Biomes.Length+1);
					Target.TerrainSystem.Biomes[Target.TerrainSystem.Biomes.Length-1] = new Biome();
				}
				if(GUILayout.Button("Remove Biome")) {
					if(Target.TerrainSystem.Biomes.Length > 0) {
						System.Array.Resize(ref Target.TerrainSystem.Biomes, Target.TerrainSystem.Biomes.Length-1);
					}
				}
			}

			if(GUILayout.Button("Generate")) {
				Target.TerrainSystem.SetHeightMap(Target.TerrainSystem.CreateHeightMap(
					Target.Seed, Target.Scale, Target.Octaves, Target.Persistance, Target.Lacunarity, Target.FalloffStrength, Target.FalloffRamp, Target.FalloffRange, Target.Offset, Target.HeightMultiplier, Target.HeightCurve
				));
				Target.TerrainSystem.SetColorMap(Target.TerrainSystem.CreateColorMap());
			}
		}
	}

	private void InspectTools() {
		using(new EditorGUILayout.VerticalScope ("Button")) {
			GUI.backgroundColor = Color.white;
			EditorGUILayout.HelpBox("Tools", MessageType.None);

			Target.ToolType = (ToolType)EditorGUILayout.EnumPopup("Type", Target.ToolType);
			Target.ToolSize = EditorGUILayout.FloatField("Size", Target.ToolSize);
			Target.ToolStrength = EditorGUILayout.FloatField("Strength", Target.ToolStrength);
			Target.ToolColor = EditorGUILayout.ColorField("Color", Target.ToolColor);
		}
	}

	private void DrawCursor() {
		Ray ray = HandleUtility.GUIPointToWorldRay(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y));
        Target.isMouseOver = Physics.Raycast(ray.origin, ray.direction, out Hit);
		Target.MousePosition = Hit.point;
        EditorUtility.SetDirty(target);
    }

	private void HandleInteraction() {
		if(Event.current.type == EventType.MouseDown && Event.current.button == 0) {
			LeftMouseDown = true;
		}
		if(Event.current.type == EventType.MouseUp && Event.current.button == 0) {
			LeftMouseDown = false;
		}
		if(LeftMouseDown && Target.isMouseOver) {
			if(Target.ToolType == ToolType.Brush) {
				Target.TerrainSystem.ModifyTexture(new Vector2(Target.MousePosition.x, Target.MousePosition.z), Target.ToolSize, Target.ToolStrength*DeltaTime, Target.ToolColor);
			} else {
				Target.TerrainSystem.ModifyTerrain(new Vector2(Target.MousePosition.x, Target.MousePosition.z), Target.ToolSize, Target.ToolStrength*DeltaTime, Target.ToolType);
			}
		}
	}

}
