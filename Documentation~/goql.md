# GoQL User Documentation


## Introduction 

GoQL is an abbreviation for "GameObject Query Language". 
It is used to construct sets of **GameObjects** from the scene hierarchy.   
The API takes a single query string as a parameter, 
and returns a set of **GameObjects** that match that query.

## Structure

A GoQL query is built using [name filters](#name-filters), [indexers](#indexers), 
[discriminators](#discriminators) and [descenders](#descenders). 
This sounds complicated, but it uses a syntax which is similar to directory paths and filenames.

### Name Filters

The name of a **GameObject** can be specified directly. E.g.:
    
    Head

will match all **GameObjects** that are named "Head".   

A name filter can also use wildcards at the beginning and end of the name, E.g.:

* `*Head`: all **GameObjects** that have a name ending with "Head".
* `Head*`: all **GameObjects** that have a name beginning with "Head".
* `*Head*`: all **GameObjects** that contain the string "Head" anywhere in their names.

Note that a name filter will match all GameObjects in the current set.  
Initially, the current set is global, and will contain all objects in the hierarchy.   
The current set can be changed using a descender.

### Descenders

A slash character (`/`) marks a descender.

    /

When GoQL reads this symbol, it will narrow the search down into the children of the current set.   
If it is the first character of the GoQL string, it will match all root objects in the scene. 

A double asterisk (`**`) is a special descender, which collects all ancestors of the current set.

    **    

This will collect every child of the current set, and the child's children, recursively until the end of hierarchy is reached.

Examples:
* `/Head`: any **GameObjects** named "Head" that are in the root of the hierarchy.
* `Head/`: all immediate children of any **GameObjects** named "Head".

### Indexers

An indexer will filter the current set using a numerical index which can specify individual indexes or a range of indexes.   
E.g.:

* `Head/[0]`: the first child of all **GameObjects** named "Head".
* `Head/[0,1,5]`: the corresponding children with specified indexes of all **GameObjects** named "Head", if they exist.   
* `Head/[-1]`: the last child of all **GameObjects named** "Head".
* `Head/[3:5]`: all children of objects named "Head" that have an index betweeen 3 and 5 (exclusive).
   

### Discriminators

A discriminator is used to filter the current set by checking for the existence of a particular component, 
material or shader.   
Discriminators are specified using angle brackets and one of these codes:
1. 't': components
2. 'm': materials
3. 's': shaders

Examples:
* `Head<t:Collider>`: all **GameObjects** named "Head" which also have a Collider component.
* `Head<m:Glow>`: all **GameObjects** that are named "Head" and have a material named "Glow"
* `Head<s:Standard>`: all **GameObjects** that are named "Head" and are using a shader named "Standard".
    
## Other Examples

* `/`: all root **GameObjects**.
* `Quad*`: all **GameObjects** who have a name beginning with "Quad".
* `Quad*/<t:AudioSource>[1]`: The second audio source component in children of all **GameObjects** 
  which have a name beginning with "Quad".
* `<t:Transform, t:AudioSource>`: all **GameObjects** that have a Transform and a AudioSource component:    
* `<t:Renderer>/*Audio*[0:3]`: the first 3 children of all **GameObjects** which
  * have parent **GameObjects** with Renderer components.
  * have "Audio" in their names
* `Cube/Quad/<t:AudioSource>[-1]`: from each **GameObject** named "Cube", select children that have a name starting with "Quad", 
  then select the last grandchild that has an AudioComponent.
* `<m:Skin>`: all **GameObjects** that use a material named "Skin".
* `/Environment/**<t:MeshRenderer>`: all ancestors of the root Environment **GameObject** that have a MeshRenderer.

    



