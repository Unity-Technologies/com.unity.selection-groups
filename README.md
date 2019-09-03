#SelectionGroups

SelectionGroups allow sets of objects to be grouped together, independent of the structure in the Hierarchy Window.

Open the SelectionGroup window via Window->General->Selection Groups.

Press the 'Add Group' button to add a group, then drag gameobjects from the Hierarchy Window to the group.

You can remove members from the group by right clicking on them and choosing remove.

You can right click on the group to open a context menu. This menu provides controls for removing, duplicating and configuring the group.

#Group Configuration
When selecting the Configure Group option from the context menu, a configuration window will open. You can change the group name, the group color and the selection query using this window.

##Selection Query
A selection query is used to automatically add gameobjects to the group based on some criteria. The first criteria is the 'Name Contains Query'. This is a simple case sensitive name match. Eg 'ameObj' will match all gameobjects that are named 'GameObject'.
The second criteria is the list of required types. This is a list of component type names (strings) which must be components on the gameobjects in order to satisfy the criteria.
The third critera is a the list of required materials. This criteria will match all gameobjects that have a renderer, and all of the materials in this list are also in the renderer.sharedMaterials propertu.
The final criteria is the list of required shaders. This criteria will match all gameobjects that have a renderer, and have materials in their sharedMaterials property which are using shaders in the required shaders list.

When selecting members in the editor window, a yellow background indicates the member has come from the selection query, while a regular background indicates the member was manually added.

##Attachments
At the bottom of the configuration window is a list of attachments. This allows arbitrary UnityEngine.Object instances to be attached to the group, for use by tools programmers to extend and use the functionality of SelectionGroups.


##Implementation Notes
SelectionGroups are multiscene and support drag and drop.
A container component exists in each scene that contains all groups for any gameobjects that are part of a group. This container also contains other data for each group, including color. Groups are identified by a unique string name. When the container has OnEnable called, it registers itself in a static list of containers, and removes from this list in the OnDisable call.

The EditorWindow uses a SelectionGroupUtility class to perform operations and aggregate all gameobjects across all scenes for each unique group name by using the above static list.

When a subscene is added, if it contains any group containers, they are automatically loaded:
 - If the container contains groups that are already present in the Editor Window, the objects contained by the new group are added to the existing group, and any data in the group is overwritten with the existing group data from the editor.
 - If the container contains groups that do not currently exist in the main scene, they are loaded and displayed in the Editor Window.

