# Selection Groups User Documentation


## Overview

Selection Groups provides a more convenient workflow in Unity by 
allowing to group a number of **GameObjects** under a common name.  
This way, we can do operations on these groups, 
instead of individual **GameObjects**.

## Getting Started

1. Open the Selection Groups window via Window > General > Selection Groups.  
   We recommend to dock this window next to the Hierarchy window.   

   ![](images/SelectionGroupsWindow.png)

1. Click “Add Group” in the Selection Groups window, and a new item will appear inside the window.   
1. Drag some **GameObjects** from the hierarchy, or assets from the Project window. 
   
   ![](images/SelectionGroupMembers.png)

1. Double click the group name, and a configuration dialog box will appear.

   ![](images/SelectionGroupConfiguration.png)

   The dialog box contains the following properties:
   1. Group name 
   1. Group color
   1. [Data Location](#data-location)
   1. GameObject Query  
      Specifies a query which will automatically include **GameObjects** from the hierarchy
      that match the query into the group.  
      For example, `/Enemy*` will select all GameObjects that are in the root of the hierarchy 
      that have names starting with `Enemy`.  
      See the [GoQL](goql.md) documentation for more information.     
   1. Enabled Toolbar Buttons  
      Enables/disables the following toolbar items in the main Selection Groups window for the group.   
      1. Eye: show or hide all the **GameObjects** in the group. 
      1. Lock: enable or disable the editing of all the **GameObjects** in the group.  

      ![](images/SelectionGroupTools.png)

## Data Location

The data of each group can be stored in one of these two locations:
1. Scene: in a scene (default).  
   This allows scripts to access the data.
2. Editor: in an asset file outside scenes.  
   This allows a decoupling between the groups and the scenes, with a tradeoff 
   that the group data cannot be used by scripts.  
   The same group can be shared by multiple scenes, 
   but the Selection Group window will only show grouped **GameObjects** that exist in the opened scene.

> When the group data is stored in the Editor, the group saves references to **GameObjects** using 
> [GlobalObjectId API](https://docs.unity3d.com/ScriptReference/GlobalObjectId.html),
> which provides project-global object identifiers, into an asset file.

