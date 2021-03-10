
GoQL User Documentation
=============================

# Introduction 
GoQL is an abbreviation for "GameObject Query Language". It is used to construct sets of GameObjects from the scene hierarchy. The API takes a single query string as a parameter, and returns a set of GameObjects that match that query.

# Creating Queries
It is possible to construct and test queries by using the Window -> General -> GoQL window. This allows you to type a query, and immediately see the objects that are selected by the query.

# Structure
A GoQL query is built using name filters, indexers, discriminators and descenders. This sounds complicated, but uses a syntax which is similar to directory paths and filenames.

## Name Filters
The name of a GameObject can be specified directly. Eg:
    
    Head

will match all GameObects that are named "Head". A name filter can also use wildcards at the beginning and end of the namem, Eg:

    *Head

will match all GameObjects that have a name ending with "Head" and

    Head*

will match all GammeObjects that have a name beginning with "Head", while

    *Head*

will match all GameObjects that contain the string "Head" anywhere in their name.

It is important to note that a Name Filter will match all GameObjects in the current set. Initially, the current set is global, and will contain all objects in the hierarchy. The current set can be changed using a descender.

## Descenders
A descender is a slash character.

    /

When GoQL reads this symbol, it will narrow the search down into the children of the current set. If it is the first character of the GoQL string, it will match all root objects in the scene. 

    /Head

will match any GameObjects named "Head" that are in the root of the hierarchy. While:

    Head/

would match all immediate children of any GameObjects named "Head".

## Indexer
An indexer will filter the current set using a numerical index which can specifiy individual indexes or a range of indexes. Eg:

    Head/[0]

Will match the first child of all GameObjects named "Head".

    Head/[0,1,5]

will match a number of children of all GameObjects named "Head", if they exist. Indexes can also be negative, Eg:

    Head/[-1]

will match the last child of all GameObjects named "Head". An indexer can also be a range, rather than single values. Eg:

    Head/[3:5]

will match all children of objects named "Head" that have an index of 3 or 4. (The range in exclusive of the final range value).

#Discriminators
A discriminator is used to filter the current set by checking for the existing of components, a material or a shader. Discriminators are specified using angle brackets and an initial discrimination code, which can be 't' for components, 'm' for materials and 's' for shaders. Eg:

    Head<t:Collider>

will match all objects named "Head" which also have a Collider component. Using a different discriminator code, we can instead match objects that are using a specific material. Eg:

    Head<m:Glow>

will match all objects that are named "Head" and have a material named "Glow", while:

    Head<s:Standard> 
    
will match all objects that are named "Head" and are using a shader named "Standard".

# Examples

Select all root objects:

    /

Select all objects who have a name beginning with "Quad":

    Quad*

Select second audio source component in children of all objects who have a name beginning with "Quad":

    Quad*/<t:AudioSource>[1]


Select all gameobjects that have a Transform and a AudioSource component: 

    <t:Transform, t:AudioSource>


Select the first 3 children of all objects that are a child of a renderer component and have "Audio" in their name: 

    <t:Renderer>/*Audio*[0:3]


From each object named "Cube", select children that have a name starting with "Quad", then select the last grandchild that has an AudioComponent.

    Cube/Quad"/<t:AudioSource>[-1]

Select all gamobjects that use the material "Skin":

    <m:Skin>
