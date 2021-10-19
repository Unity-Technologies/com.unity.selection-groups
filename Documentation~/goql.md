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

A name filter can also use wildcards at the beginning and end of the name, e.g.:

* `*Head`: all **GameObjects** that have names ending with "Head".
* `Head*`: all **GameObjects** that have names beginning with "Head".
* `*Head*`: all **GameObjects** that contain the string "Head" anywhere in their names.

Limitations
* E*v*t: only single inner wildcard is supported 
* *E*t*: can't be used together with beginning or ending wildcard

Note that a name filter will look for matching **GameObjects** in the current applicable set.  
Initially, the current applicable set is global, 
and will contain all objects in the hierarchy.   
The applicable set can be changed by using [a descender](#descenders).

### Descenders

A descender is defined by a slash character (`/`).

    /

When GoQL reads this symbol, it will narrow the search down into 
the children of the current applicable set.   
If it is the first character of the GoQL string, it will match all root objects in the scene. 

A double asterisk (`**`) is a special descender, which matches all descendants of the applicable set.

Examples:
* `/Head`: any **GameObjects** named "Head" that are in the root of the hierarchy.
* `Head/`: all immediate children of any **GameObjects** named "Head".

### Indexers

An indexer will filter the current set using a numerical index which can be an individual index or a range of indexes.   
E.g.:

* `Head/[0]`: the first child of all **GameObjects** named "Head".
* `Head/[0,1,5]`: the children with specified indexes of all **GameObjects** named "Head", if they exist.   
* `Head/[-1]`: the last child of all **GameObjects** named "Head".
* `Head/[3:5]`: the children of **GameObjects** named "Head" that have indexes betweeen 3 and 5 (exclusive).
   

### Discriminators

A discriminator is used to filter the current set by checking for the existence of a particular component, 
material or shader.   
Discriminators are specified using angle brackets and one of these codes:
1. 't': component
2. 'm': material
3. 's': shader

Examples:
* `Head<t:Collider>`: all **GameObjects** named "Head" which also have a Collider component.
* `Head<m:Glow>`: all **GameObjects** that are named "Head" and use materials named "Glow"
* `Head<s:Standard>`: all **GameObjects** that are named "Head" and are using "Standard" shader.

### Negation


* /!*ead/!C* : from root gameobjects that don't have names ending with ead, pick their children which don't start with C 
* /!Head* : root gameobjects that don't begin with  head
* /!*Head*: root gameobjects that don't have head anywhere
* /!H*d: root gameobjects that don't begin with h and end with d
* !Head: all gameobjects which don't have head as their names
* /Head/!Cube/*: from all root gameobjects which names are head, search their children which names are not Cube, 
  and get their children

* /Head/!Cube: children of head that are not cube
* /Head/!C*: children of head that don't begin with C

#### Limitations

Multiple negations are not supported in the same set, for example: `/!H!ead*`

    
## Other Examples

* `/`: all root **GameObjects**.
* `Quad*`: all **GameObjects** which have names beginning with "Quad".
* `Quad*/<t:AudioSource>[1]`: The second audio source component in children of all **GameObjects** 
  which have names beginning with "Quad".
* `<t:Transform, t:AudioSource>`: all **GameObjects** that have Transform and AudioSource components.    
* `<t:Renderer>/*Audio*/[0:3]`: the first 3 children of all **GameObjects** which:
  * have parent **GameObjects** with Renderer components.
  * have "Audio" in their names
* `Cube/Quad/<t:AudioSource>[-1]`: from each **GameObject** named "Cube", 
  select children that have names starting with "Quad", 
  then select the last grandchild that has an AudioSource component.
* `<m:Skin>`: all **GameObjects** that use materials named "Skin".
* `/Environment/**<t:MeshRenderer>`: all descendants of root **GameObjects** named "Environment" 
  that have MeshRenderer components.
* `/Head*!*Unit`: all root gameobjects that have a name that starts with "Head", but do not end with "Unit".

    



