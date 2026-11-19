# Character Visual Pipeline

## Decision

The current prototype direction is **Godot-native 2D modular artwork**, composed from independently authored visual layers and animated with a small skeletal rig where practical.

Customization follows a **hybrid preset + constrained-slider model**:
- Presets select a compatible base (body, race, proportions and supported rig).
- Selectable parts replace or overlay visual layers (face details, hair, clothing, accessories).
- Sliders expose only parameters supported by the selected base and its authored assets. A slider must not promise arbitrary deformation when the artwork has no corresponding morph/deformation support.

The presentation data remains separate from gameplay Character state. The visual renderer consumes a character appearance selection; it does not own stats, actions, equipment rules or narrative decisions.

## Why 2D modular instead of 3D

The desired presentation is illustrated anime-style character art, with emphasis on portrait expression, full-body appearance, clothing and customization. A 2D pipeline keeps the project focused on illustration and reuses the same authored parts across profile, dialogue and location presentation.

3D would be preferable if the game required unrestricted camera rotation, many arbitrary viewpoints, frequent full-body action animation or extensive retargeting. It also adds modeling, topology, UV, materials, texturing, lighting and 3D rigging to the learning and production workload. Those capabilities are not required to prove this game's current character presentation.

## Why not make Live2D the foundation

Live2D is a strong candidate for expressive portrait motion, but it does not remove the need to author and prepare layered artwork. It also introduces a separate rigging workflow and a Godot integration/licensing decision. Keep it as a possible portrait renderer later; do not couple identity, appearance data or asset selection to Live2D-specific types.

## Target presentation architecture

- **Character identity:** stable identity and authored character metadata.
- **Appearance selection:** body preset, face, hair, colors and other supported visual choices.
- **Outfit selection:** clothing and cosmetic accessories.
- **Gameplay equipment:** gameplay-facing items and effects; not the same thing as cosmetic clothing.
- **Visual definition/catalog:** maps stable part IDs and compatible combinations to authored assets.
- **Character renderer:** composes the selected assets for a target context (full body, portrait or preview).
- **Animation rig:** owns bones, pivots and animation clips for the selected compatible base.

Domain gameplay rules must not reference Godot nodes, textures, scenes, bones or rendering resources. Presentation/Infrastructure owns the mapping from stable appearance IDs to Godot resources.

A portrait may use a separately authored crop or expression illustration, but it must be selected from the same appearance identity/configuration. Do not maintain a second independent character identity for dialogue.

## Asset composition

The intended layer families are:

1. Body base: body silhouette and base proportions.
2. Face: face shape and replaceable facial details.
3. Hair: back hair, front hair and optional variants.
4. Outfit: clothing layers with explicit slot and compatibility rules.
5. Accessories: optional items such as glasses, earrings or headwear.
6. Expression: eye/mouth/brow variants or rigged facial parts.

Each part needs a stable ID, a documented canvas size, pivot/origin convention, layer order and compatibility tags. Parts that cannot fit a base or rig must be rejected by catalog validation rather than silently rendered at incorrect offsets.

For the first prototype, prefer a small set of hand-authored parts with one visible selectable variation. Do not create dozens of placeholder parts or a large catalog before the authoring workflow is proven.

## Presets and sliders

Presets should own compatibility and defaults. A preset can declare supported body base, rig, race-specific parts and adjustable parameters.

Sliders are parameter definitions, not arbitrary free-form transforms. Each parameter should specify:
- stable parameter ID;
- minimum, maximum and default;
- the body presets that support it;
- the visual assets, bones or deformation data that consume it;
- whether the value is continuous or stepped.

Start with simple, visually safe parameters such as height/scale or a few authored proportion variants. True body morphing requires additional art/deformation work and must be proven separately. Do not expose sliders that only stretch a sprite and visibly distort clothing or anatomy.

## Godot implementation direction

- Use ordinary 2D textures/sprites as independently authored layers.
- Use Node2D composition for ordering and transforms.
- Evaluate Skeleton2D / Bone2D for shared body motion; use AnimationPlayer for a tiny idle or breathing proof only when the art is separated and pivoted correctly.
- Keep the preview scene isolated until the pipeline is verified.
- Avoid putting asset paths in CharacterViewModel or gameplay Character classes.
- Do not add a generic plugin framework or full creator UI in the prototype.

## Suggested authoring workflow for the user

A practical learning sequence is:

1. **Draw one simple base:** create a front-facing or three-quarter full-body character on a consistent canvas. Focus on silhouette and readable proportions rather than detail.
2. **Separate layers:** body, hair-back, face, hair-front and one clothing layer. Keep every layer on the same canvas dimensions so alignment is deterministic.
3. **Set pivots and naming:** use consistent names and origins. For animation, separate limbs or body regions at joints rather than cutting through arbitrary pixels.
4. **Export transparent PNGs:** preserve canvas size and transparency. Avoid resizing individual parts after export.
5. **Import into Godot:** assemble the layers in a preview scene and verify ordering, scale and offsets.
6. **Add one variation:** swap hair or outfit by changing a stable part ID, not by editing renderer code.
7. **Rig only after composition works:** add a small number of bones, weight/separation decisions and one simple motion. A badly separated illustration is harder to rig than a simple, deliberately prepared one.

Krita is a suitable free starting point for painted raster layers. Aseprite is useful for pixel-art workflows, but is not required for painted anime-style art. The user can start with Krita and Godot; paid illustration software is not a prerequisite.

## Learning difficulty

- **Basic 2D drawing and layer separation:** approachable, but producing polished anime character art is a substantial art skill.
- **Godot layer composition:** relatively approachable; mostly scene hierarchy, texture resources, ordering and transforms.
- **Character rigging:** intermediate; pivots, joint separation, deformation artifacts and animation timing take practice.
- **High-quality customization at scale:** difficult; compatibility between body types, outfits, hair and poses creates a combinatorial content-production problem.

The user can learn this pipeline. The first independent practice should be one small hair or clothing layer, not a complete character. AI-generated or placeholder art may help test layout, but should not replace learning how source layers, pivots and compatibility work.

## Explicit future decisions

Do not decide these prematurely:
- final illustration style and camera angle;
- whether portraits later use Live2D;
- how many body presets and races ship;
- whether continuous body morphing is worth its art cost;
- whether all outfits support all body presets.

These decisions should follow a working small asset set and a hands-on authoring test.
