# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.8.0-preview] - 2022-04-28

### Added
* feat: implemented the IList<Object> interface in SelectionGroup 
* feat: query active/inactive GameObjects in GoQL
* feat: allow group creation using drag and drop

### Changed
* package: upgrade min supported Unity version to 2020.3
* change: make the members of SelectionGroup into GameObject
* deps: use com.unity.film-internal-utilities@0.17.0-preview

### Removed
* remove: SelectionGroupConfigurationDialog class

## [0.7.4-preview] - 2022-04-15

### Changed
* deps: use com.unity.film-internal-utilities@0.14.2-preview 

### Fixed
* fix: support com.unity.quicksearch@3.x.x on Unity 2020.3.x

## [0.7.3-preview] - 2022-04-11

### Added
* doc: update the group creation section in Getting Started and SelectionGroupsWindow pages

### Changed
* change: use a dropdown button to create a group or group from selection
* deps: use com.unity.film-internal-utilities@0.14.1-preview 

### Fixed
* fix: the use of negative index relative to child count in GoQL

## [0.7.2-preview] - 2022-02-16

### Added
* api: make SelectionGroupToolAttribute public
* doc: add Quick Search Integration section
* doc: add documentation on Project Settings
* doc: add "moving group members" operation in the Selection Groups Window documentation

### Changed
* change the format of EditorToolStates of groups
* doc: update the exclusion documentation on GoQL
* doc: change the inspector documentation to a table

### Fixed

* fix: prevent duplicate members in groups 
* fix: freezes when there is an unclosed angle bracket

## [0.7.1-preview] - 2022-02-10

### Fixed
* fix: exclude hidden GameObjects from GoQL results 
* fix: refresh query results after changing visibility settings in the Hierarchy

## [0.7.0-preview] - 2022-02-08

### Added
* feat: add an option to hide SelectionGroups GameObjects in the hierarchy
* feat: add settings to set the default toolbar buttons for new SelectionGroup 
* feat: show hidden icon for members which are hidden in scene
* feat: use ctrl to move group members when dragging them in the window
* feat: exclusion operator for GoQL indexers
* feat: integrate GoQL with SearchProvider (QuickSearch)
* api: add an API to add/remove object to/from SelectionGroup

### Changed
* internal : rename API to create new SelectionGroup
* deps: update dependency to com.unity.film-internal-utilities@0.13.0-preview

### Fixed
* fix: make the visibility toggle button set visibility based on the first member 


## [0.6.3-preview] - 2022-01-18

### Added
* doc: add SelectionGroups window and installation documents

### Fixed
* fix: keyboard shortcut to delete group/member


## [0.6.2-preview] - 2022-01-11

### Fixed
* fix: undo deleting a group didn't work 
* fix: make sure that dragging groups is preceded by selecting the group to drag
* fix: closing a scene should not unregister groups
* fix: double clicking to select all members was broken 

## [0.6.1-preview] - 2022-01-11

### Fixed
* fix: show the group in the inspector during mouse up event
* fix: deleting a group gameObject didn't clean up the bookkeeping

## [0.6.0-preview] - 2022-01-07

### Added
* feat: allow manual reordering of groups in SelectionGroupEditor window
* feat: additional mouse handling for members in the SelectionGroupEditorWindow (Ctrl, Ctrl+Shift)
* feat: double click on the group to select all its members 
* feat: serialize editor tools settings for SelectionGroups 

### Changed
* change: show SelectionGroup configuration in the inspector window
* change the handling of "SelectAll", "SelectAll", "InvertSelection", "SoftDelete" commands]
* change: reduce the width of group color in SelectionGroupEditor window
* change: try to assign a different name when adding a new group 
* rename namespaces
* deps: update dependency to com.unity.film-internal-utilities@0.12.4-preview

### Fixed
* fix: keep the order of SelectionGroups
* fix: display group color using its alpha as well
* fix: smoothen mouse hovering in the SelectionGroup window
* fix: disable "Remove From Group" context when clicking on auto-filled groups
* fix: destroy GameObject when its SelectionGroup component is destroyed 
* fix: hide transform properties of SelectionGroup in the inspector 
* fix: prevent adding SelectionGroup and SelectionGroupManager components manually

### Removed
* remove: EditorSelectionGroup (editor mode)
* remove "update query results" context menu
* remove SelectionGroupConfigurationDialog

## [0.5.5-preview] - 2021-11-19

### Fixed
* fix: allow adding objects from different scenes to a selection group

## [0.5.4-preview] - 2021-10-25

### Changed
* removed SG_ prefix by default to selection groups in scene.

### Fixed
* fix: opening a new scene while having the SG configuration window caused errors

## [0.5.3-preview] - 2021-10-21

### Added
* doc: add/update the wildcard and exclusion in the GoQL documentation

### Fixed
* fix: suppress GoQL log if not in debug mode 

## [0.5.2-preview] - 2021-10-12

### Changed

* add SG_ prefix by default to selection groups in scene.

### Fixed

* fix: support undo for goql query editing

## [0.5.1-preview] - 2021-09-22

### Fixed

* Fix subscene not loaded error 

## [0.5.0-preview] - 2021-09-14

### Added
* goql: support for wildcards in the middle
* goql: added not (!) operator


### Fixed
* goql bug fixes for tokenizing

## [0.4.3-preview] - 2021-08-27

### Fixed
* fix GoQL Query results not being updated when GameObject names are changed

## [0.4.2-preview] - 2021-08-03

### Changed
* doc: change changelog format to adhere to Semantic Versioning 

### Fixed
* fix group text colors which were sometimes shown in black while using Unity Pro
* fix foldout/toggle icon which sometimes did not appear in the SelectionGroups window

## [0.4.1-preview] - 2021-06-01

### Fixed
* fix: fix chopped names in SelectionGroups window.
* fix: added common '_' and '-' chars to acceptable string chars.

## [0.4.0-preview] - 2021-06-01

### Added
* open SelectionGroup and SelectionGroupDataLocation to public 

### Changed
* opt: optimize goql query events by coalescing consecutive hierarchy change events into one.
* ui: change the text color on non-pro skin for headings

## [0.3.3-preview] - 2021-04-26

### Changed
* doc: misc updates

### Fixed
* fix: GoQL caching types twice.

## [0.3.2-preview] - 2021-03-17

### Changed
* hide legacy members of SelectionGroup in the inspector 

### Fixed
* fix: warning "the field 'SelectionGroup.sgVersion' is assigned but its value is never used" 

## [0.3.1-preview] - 2021-03-11

### Fixed
* fix: deserialize the SelectionGroup members in older version

## [0.3.0-preview] - 2021-03-10

### Added
* doc: add goql documentation

### Changed
* optimisation 
* changed event handling and repaint to avoid GC
* internal: change public APIs to internals 
* set the default SelectionGroup type to Scene 
* rename GoQL assembly for consistency

### Fixed
* fix: the labels in SelectionGroupWindow were not displayed correct in the second screen 
* fix: remove null members in SelectionGroup
* fix: show the tool buttons in an aligned manner 

## [0.2.3-preview] - 2021-02-16

### Changed
* UX improvements (Select All, Select None)
* Enable SelectionGroups Editor Window in Play mode.

## [0.2.2-preview] - 2021-01-26

### Added
* Added GetMemberComponents method to ISelectionGroup interface.

## [0.2.1-preview] - 2021-01-07

### Changed
* Changed storage of Runtime Groups to always exist in a scene.
* Changed Editor and Runtime classes to use a common interface (ISelectionGroup)

### Removed
* Removed editor to scene selection group synchronisation (no longer needed)

## [0.1.1-preview] - 2020-12-01

### Fixed
* Fixed update behaviour on GoQL queries.

### Removed
* Removed debug buttons.

## [0.1.0-preview] - 2020-10-07

### Added
* Editor API docs. 
* Runtime API docs.
* GoQL cleanup + docs

### Fixed
* Reapply package meta fixes

### Removed
* removed redundant classes

## [0.0.7-preview] - 2020-09-29

### Fixed
* Fixed license

## [0.0.6-preview] - 2020-09-29

### Fixed
* Fixed package info.

## [0.0.5-preview] - 2020-09-20

### Added
* Merged GoQL package source.

## [0.0.2-preview] - 2019-12-26

### Changed
* Refactored to store selection group data outside the scene hierarchy.

## [0.0.1-preview] - 2019-08-29

### Added
* The first release of *Selection Groups \<com.unity.selection-groups\>*.

