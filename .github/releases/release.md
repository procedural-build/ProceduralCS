# Procedural Grasshopper plugin.
Please find a working example by going to our [demo](https://github.com/procedural-build/demos) repo.

## Components Updated in this Release
The following components have been updated since our last release and it is advised to replace them in any canvas you have.
Just because the components have been updated, that doesn't mean that the only ones won't work any more, but there might be issues with added/removed/changed input, outputs, descriptions and tooltips.

## Installation

We provide the option to install the Procedural Grasshopper plugin through the YAK package manager. 
To install with YAK, just download and open the [ComputeVWT-DEMO.gh](https://github.com/procedural-build/demos/blob/master/VWT/ComputeVWT-DEMO.gh?raw=true) file and click install when the package manager opens.
We also have a [video](https://www.youtube.com/embed/oQU_Uke5368) walking through the process 

## Update with YAK

1. Write `TestPackageManager` in the Rhino console. 
2. Select the `Installed` tab. 
3. Click on `ProceduralCS`. 
4. Click `Update`. 
5. Restart Rhino

 ![alt text](https://github.com/procedural-build/ProceduralCS/raw/master/.github/releases/UpdateYAK.gif "Update ProceduralCS with YAK")

### Manual Installation/Update (Not recommended)
If using the YAK package manager doesn't work you can install the package manually by downloading the [ComputeGH.gha](https://github.com/procedural-build/ProceduralCS/blob/{{RELEASE_VERSION}}/dist/ComputeGH.gha?raw=true) and [ComputeCS.dll](https://github.com/procedural-build/ProceduralCS/blob/{{RELEASE_VERSION}}/dist/ComputeCS.dll?raw=true) putting them in the Grasshopper Components folder.
 That folder can be found in Edit > Special Folders > Components.
 
 ![alt text](https://github.com/procedural-build/ProceduralCS/raw/master/.github/releases/GrasshopperLibraries.png "Find the Grasshopper Components folder.")
 
 **Remember to unblock ProceduralCS.dll and ProceduralGH.gha**
 
 ![alt text](https://github.com/procedural-build/ProceduralCS/raw/master/.github/releases/Unblock.png "Unblock ProceduralCS.dll and ProceduralGH.gha")

This release is only working with Rhino 6!
