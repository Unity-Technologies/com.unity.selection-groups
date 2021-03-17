# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.3.2-preview] - 2021-03-17
* fix: warning "the field 'SelectionGroup.sgVersion' is assigned but its value is never used" 
* fix: hide legacy members of SelectionGroup in the inspector 

## [0.3.1-preview] - 2021-03-11
* fix: deserialize the SelectionGroup members in older version

## [0.3.0-preview] - 2021-03-10

* optimisation 
* changed event handling and repaint to avoid GC
* internal: change public APIs to internals 
* fix: the labels in SelectionGroupWindow were not displayed correct in the second screen 
* fix: remove null members in SelectionGroup
* fix: set the default SelectionGroup type to Scene 
* fix: show the tool buttons in an aligned manner 
* doc: add goql documentation
* chore: rename GoQL assembly for consistency

## [0.2.3-preview] - 2021-02-16

* UX improvements (Select All, Select None)
* Enable SelectionGroups Editor Window in Play mode.

## [0.2.2-preview] - 2021-01-26

* Added GetMemberComponents method to ISelectionGroup interface.

## [0.2.1-preview] - 2021-01-07

* Changed storage of Runtime Groups to always exist in a scene.
* Changed Editor and Runtime classes to use a common interface (ISelectionGroup)
* Removed editor to scene selection group synchronisation (no longer needed)

## [0.1.1-preview] - 2020-12-01

* Fixed update behaviour on GoQL queries.
* Removed debug buttons.

## [0.1.0-preview] - 2020-10-07

* Editor API docs. 
* Runtime API docs.
* GoQL cleanup + docs
* removed redundant classes
* Reapply package meta fixes

## [0.0.7-preview] - 2020-09-29

* Fixed license

## [0.0.6-preview] - 2020-09-29

* Fixed package info.

## [0.0.5-preview] - 2020-09-20

* Merged GoQL package source.

## [0.0.2-preview] - 2019-12-26

* Refactored to store selection group data outside the scene hierarchy.

## [0.0.1-preview] - 2019-08-29

* The first release of *Selection Groups \<com.unity.selection-groups\>*.

