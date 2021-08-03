# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

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

