types = {
    "root":
    {
        1:("chunk", "PrefabData"),
        3:("Prefab", "Prefab"),
        5:("Electrical", "Electrical"),
        10:("Buildings", "Buildings"),
        11:("string", "checksum"),
    },
	"Prefab": {
    
        1:("string", "Category"),
        2:("uint32", "ID"),
        3:("Vector3", "Position"),
        4:("Vector3", "Rotation"),
        5:("Vector3", "Scale"),
        
     },
     "Buildings": {
    
        1:("PrefabINT", "Prefab and building ID"),
        
     },
     "PrefabINT": {
    
        1:("Prefab", "Prefab"),
        2:("int32", "Building number"),
        
     },
     "Vector3": {
     1:("float", "X"),
     2:("float", "Y"),
     3:("float", "Z"),
     },
     
     "Electrical": {
     
     1:("Circuit", "Circuit"),
     },
     
     "Circuit": {
     1:("string", "path"),
     2:("Vector3", "wiring"),
     3:("subcircuit", "connection"),
     4:("subcircuit", "connection"),
     5:("int32", "cardcolor"),
     6:("int32", "flow"),
     7:("float", "setting"),
     11:("chunk", "flow"),
     14:("chunk", "flow"),
     16:("int32", "flow"),
     17:("chunk", "flow"),
     },
     
     "subcircuit": {
     1:("string", "path"),
     2:("Vector3", "wiring"),
     3:("int32", "flow"),
     4:("int32", "flow"),
     5:("int32", "fluidflow"),
     },
     
}
			