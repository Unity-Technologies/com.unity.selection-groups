# GoQL User Documentation


## Introduction 

GoQL is an abbreviation for "GameObject Query Language". 
It is used to construct sets of **GameObjects** from the scene hierarchy.   
The API takes a single query string as a parameter, 
and returns a set of **GameObjects** that match that query.

## Structure

A GoQL query is composed using [name filters](#name-filters), [indexers](#indexers), 
[discriminators](#discriminators) and [descenders](#descenders).   
This sounds complicated, but the syntax is similar to directory paths and filenames.

### Name Filters

The name of a **GameObject** can be specified directly. E.g.:
    
    Head

will match all **GameObjects** that are named "Head".   

A name filter can also use wildcards at the beginning, middle, or end of the name, e.g.:

|**GoQL** |**Description** |
|:--------|:---|
|`*Head`  |All **GameObjects** that have names ending with "Head".|
|`Head*`  |All **GameObjects** that have names beginning with "Head".|
|`*Head*` |All **GameObjects** that contain the string "Head" anywhere in their names.|
|`H*d`    |All **GameObjects** that begin with "H" and end with "d".|

Note that a name filter will look for matching **GameObjects** in the current applicable set.  
Initially, the current applicable set is global, 
and will contain all objects in the hierarchy.   
This applicable set can be changed by using [a descender](#descenders).


> Currently, middle wildcards have the following limitations:
> 1. Only one middle wildcard is allowed in each applicable set, e.g: 
>   * `/H*d/C*d` is supported, but
>   * `/H*d*Tail` is not supported
> 2. A middle wildcard can't be used together with either a beginning or ending wildcard 
>   in the same applicable set, e.g: 
>   * `/*d/C*d/T*` is supported, but
>   * `/*d*T*` is not supported

#### Exclusions

An exclusion is defined by an exclamation point (`!`).  
Writing a name filter after `!` will exclude **GameObjects** that match the name filter 
in the current applicable set, e.g.:

|**GoQL**           |**Description** |
|:------------------|:---|
|`!Head*`           |All **GameObjects** that do not have names beginning with  "Head" |
|`Hea*!*d`          |All **GameObjects** which names begin with  "Hea", but don't end with "d" |
|`Hea*!Heat`        |All **GameObjects** which names begin with  "Hea", but are not "Heat" |
|`Hea*!Heat!Head`   |All **GameObjects** which names begin with  "Hea", but are neither "Heat" nor "Head" |
|`!H*d`             |All **GameObjects** which names don't begin with "H" and don't end with "d" |


### Descenders

A descender is defined by a slash character (`/`).

When GoQL reads this symbol, it will narrow the search down into 
the children of the current applicable set.   
If it is the first character of the GoQL string, it will match all root objects in the scene. 

A double asterisk (`**`) is a special descender, which matches all descendants of the applicable set.

Examples:

|**GoQL**    |**Description** |
|:-----------|:---|
|`/Head`     |Any **GameObjects** named "Head" that are in the root of the hierarchy. |
|`Head/`     |All immediate children of any **GameObjects** named "Head".|

### Indexers

An indexer will filter the current set using a numerical index which can be an individual index or a range of indexes.   
E.g.:

|**GoQL**      |**Description** |
|:-------------|:---|
|`Head/[0]`    |The first child of all **GameObjects** named "Head". |
|`Head/[0,1,5]`|The children with specified indexes of all **GameObjects** named "Head", if they exist.|
|`Head/[-1]`   |The last child of all **GameObjects** named "Head". |
|`Head/[3:5]`  |The children of **GameObjects** named "Head" that have indexes betweeen 3 and 5 (exclusive).|
   

### Discriminators

A discriminator is used to filter the current set by checking for the existence of a particular component, 
material or shader.   
Discriminators are specified using angle brackets and one of these codes:
1. 't': component
2. 'm': material
3. 's': shader

Examples:

|**GoQL**            |**Description** |
|:-------------------|:---|
|`Head<t:Collider>`  |All **GameObjects** named "Head" which also have a Collider component.|
|`Head<m:Glow>`      |All **GameObjects** that are named "Head" and use materials named "Glow" |
|`Head<s:Standard>`  |All **GameObjects** that are named "Head" and are using "Standard" shader. |

## Other Examples

|**GoQL**            |**Description** |
|:----------------------------------|:---|
|`/`                            |All root **GameObjects**.|
|`Quad*`                        |All **GameObjects** which have names beginning with "Quad".|
|`Quad*/<t:AudioSource>[1]`     |The second child with AudioSource of all **GameObjects** which have names beginning with "Quad".|
|`<t:Transform, t:AudioSource>` |All **GameObjects** that have Transform and AudioSource components.|
|`<t:Renderer>/*Audio*/[0:3]`   |The first 3 children of all **GameObjects** which: <br/>  * have parent **GameObjects** with Renderer components. <br/>  * have "Audio" in their names |
|`Cube/Quad/<t:AudioSource>[-1]`|From each **GameObject** named "Cube",  select children that have names starting with "Quad",  then select the last grandchild that has an AudioSource component.|
|`<m:Skin>`                     |All **GameObjects** that use materials named "Skin".|
|`/Env/**<t:MeshRenderer>`      |All descendants of root **GameObjects** named "Env"  that have MeshRenderer components.|
|`/!*Head*`                     |Root **GameObjects** that don't have "Head" anywhere|
|`/!*ead/!C*`                   |From root **GameObjects** that don't have names ending with "ead", select their children which don't start with "C"|
|`/Head/!Cube/*`                |From root **GameObjects** which names are "Head", select their children which are not named "Cube", then select their children.|
|`/Head*!*Unit`                 |All root **GameObjects** which names start with "Head", but do not end with "Unit".|

    



