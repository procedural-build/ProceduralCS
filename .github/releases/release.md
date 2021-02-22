# Procedural Grasshopper plugin.

Please find a working example by going to our [demo](https://github.com/procedural-build/demos) repo.

## Components Updated in this Release
The following components have been updated since our last release and it is advised to replace them in any canvas you have.
Just because the components have been updated, that doesn't mean that the only ones won't work any more, but there might be issues with added/removed/changed input, outputs, descriptions and tooltips.
            
## New Components
{{ NEW_COMPONENTS }}
            
## Updated Components
{{ UPDATED_COMPONENTS }}
            
## Installation

We provide the option to install the Procedural Grasshopper plugin through the YAK package manager. 
To install with YAK, just download and open the [ComputeVWT-DEMO.gh](https://github.com/procedural-build/demos/blob/master/VWT/ComputeVWT-DEMO.gh?raw=true) file and click install when the package manager opens.
We also have a [video](https://www.youtube.com/embed/oQU_Uke5368) walking through the process 

## Update with YAK

1. *Rhino 6:* Write `TestPackageManager` in the Rhino console. *Rhino 7:* Write `PackageManager`
2. Select the `Installed` tab. 
3. Click on `ProceduralCS`.
4. Click `Update`. 
5. Restart Rhino

![alt text](https://github.com/procedural-build/ProceduralCS/raw/master/.github/releases/UpdateYAK.gif "Update ProceduralCS with YAK")

### Manual Installation/Update (Not recommended)
If using the YAK package manager doesn't work you can install the package manually by going to [Food4Rhino](https://www.food4rhino.com/app/proceduralcs) and downloading the package there.
Put the downloaded in the Grasshopper Components folder.
That folder can be found in Edit > Special Folders > Components.
We recommend that you create a separate folder ther called `ProceduralCS`, where you store the files.

![alt text](https://github.com/procedural-build/ProceduralCS/raw/master/.github/releases/GrasshopperLibraries.png "Find the Grasshopper Components folder.")

**Remember to unblock the files**

![alt text](https://github.com/procedural-build/ProceduralCS/raw/master/.github/releases/Unblock.png "Unblock ProceduralCS.dll and ProceduralGH.gha")

This release works with both Rhino 6 and 7