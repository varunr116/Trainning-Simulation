# Trainning-Simulation# Warehouse Safety Training Simulation

Unity-based interactive training application with SCORM integration for warehouse safety education.

## Build Instructions

### Unity Setup
1. Open project in Unity 6 or newer
2. Install required packages:
   - Universal Render Pipeline
   - Video Player package
3. Set build target to WebGL or Windows Standalone

### WebGL Build
1. Go to File > Build Settings
2. Select WebGL platform
3. Player Settings > Publishing Settings > Compression Format: Gzip
4. Build to desired folder
5. Copy SCORMBridge.jslib to Assets/Plugins/WebGL/ if missing

### Desktop Build
1. Select Windows/Mac/Linux platform
2. Build normally - no special configuration needed

### SCORM Package Creation
1. Build WebGL version
2. Copy imsmanifest.xml and metadata.xml to build folder
3. Update manifest file with actual build file names
4. Zip all contents with manifest at root level

## SCORM Implementation Summary


### What Was Built
- **SCORMClient.cs**: Handles Unity-LMS communication
- **SCORMBridge.jslib**: JavaScript bridge for WebGL-browser communication
- **LMSManager.cs**: Coordinates LMS operations and fallbacks

### Standards Support
- SCORM 1.2 and SCORM 2004 compatibility
- Automatic API detection across different LMS iframe setups
- Simulation mode for testing without LMS

### Progress Tracking
- Equipment inspection: 40% of total score
- Item collection: 40% of total score  
- Quiz assessment: 20% of total score
- Real-time progress reporting to LMS
- Session persistence across browser refreshes

### Testing
- Validated on SCORM Cloud platform
- Package imports correctly and tracks progress
- Works with standard LMS systems

Link for testing WebGL - https://app.cloud.scorm.com/sc/InvitationConfirmEmail?publicInvitationId=5b095505-5364-48cc-b52e-9247864f272d

## Git Workflow

This was a solo project with standard Git practices:
- Main branch for stable releases
- Feature branches for major additions
- Regular commits with descriptive messages
- Used GitHub for hosting video assets

## Known Issues and Limitations

### Lighting
- **Major Issue**: Laptop crashed during light baking in warehouse scene
- Lost all baked lighting data with no time to rebuild
- Warehouse scene appears darker than intended
- Please evaluate functionality over visual quality

### UI Polish
- Some interface elements are unfinished
- Focus was on core functionality over visual refinement
- Main systems work correctly despite UI limitations

### Video Playback
- WebGL uses streaming URL from GitHub
- Desktop uses local VideoClip
- Original MP4 doesn't work directly in WebGL builds

### Browser Compatibility
- Tested primarily on Chrome and Firefox
- WebGL performance varies by hardware
- Requires modern browser with WebGL 2.0 support

### Audio
- Some placeholder audio clips not implemented
- Core audio feedback works correctly
- Scene transitions may have audio gaps

## Quick Start

1. **Desktop**: Run WarehouseTraining.exe
2. **WebGL**: Upload SCORM package to LMS or open index.html in browser
3. **Testing**: Use SCORM Cloud for LMS validation

## Project Structure

- Scene 1: Classroom inspection training
- Scene 2: Warehouse collection simulation  
- Quiz: Knowledge assessment
- Certificate: Completion validation with scoring
