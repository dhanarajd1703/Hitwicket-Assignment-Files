HITWICKET TECHNICAL ART ASSIGNMENT
This repository contains the submission for the Hitwicket Technical Art assignment, which includes:

-Cel Shader Implementation
-Jersey Color Editor Tool

The File Structure is:

HITWICKET ASSIGNMENT/
│
├── Hitwicket Scene
├── Assets/
│   ├── Project Assets/
│   │   └── Shaders/
│   │       └── HitWicket_Cel_Shader
│   └── Editor/
│       └── JerseyColorBatchTool.cs
│
Hitwicket Assignment Documentation/
│
├── HITWICKET ASSIGNMENT DOCUMENTATION
└── Hitwicket Cel Shader Preview
|__ Hitwicket Jersey Tool Preview

Part 1 – Cel Shader

Scene

Open Hitwicket Scene inside the Unity project.

Character with default material
Character with Cel Shader applied

▶ How to Use the Shader
Select any material>In the Inspector> change the shader to:HitWicket_Cel_Shader

Adjust exposed parameters:
-Color
-Ambient Strength
-Specular Color
-Smoothness
-Fresnel Color
-Fresnel Size
-Lighting Cutoff
-Light Falloff Amount

Part 2 – Jersey Color Editor Tool
How to Open the Tool

In Unity: Tools → Jersey Color Batch Tool

>Tool Features
-Auto-detection of jersey materials (Jersey_Cel_Mat)
-Batch editing across multiple selected objects
-Live Preview toggle
-Create Instance Before Edit (non-destructive workflow)
-Random team color generator
-Reset to previous color
-Preset system (add, apply, delete)
-Custom color swatches
-Team color history (reapply, delete, clear)
-Persistent data storage using JSON via EditorPrefs
-The tool modifies the _Color property on the cel shader.

In the documentation and Video in Hitwicket Assignment Documentation Folder. I have explained things further in detail. 

THANK YOU FOR THE OPPORTUNITY
