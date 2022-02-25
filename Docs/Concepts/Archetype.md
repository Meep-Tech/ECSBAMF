Archetypes are the heart and soul of [[XBam]]. They are both [[Flyweight Data]] Stores, and [[Factory|Factories]] that produce [[Model]]s. An Archetype's purpose is to hold Static/Immutable data and logic, and to produce Models using a provided collection of [[Parameter]]s.

By default, Archetypes act like Singletons; Only one archetype of each Class Type is allowed to exist at any given time.

# Creating A New Tree of Achetypes
Only [[Base Model Type]]s that extend [[Model+2]] can have branching trees of Archetypes. Alternately, [[Model+1]] types only have a single Root Archetype which acts as a [[Simple Builder Factory]] which can be customized to produce multiple Model Types if one desires.

[[Base Archetype]]s and their [[Child Archetype|children]] are organized into [[Archetype Tree]]s. The Base Archetype is the root of this tree, and all other Archetypes in this Tree/Family inherit from that Base Archetype. 
The Base Archetype has a [[Base Model Type]], which is the most basic type of [[Model]] that this Archetype can produce as a Factory. All Models produced by any Archetype in this family will inherit from this Base Model Type.
All classes that inhert from the Base Archetype are built during startup by the [[Loader]] and are included in the [[Archetype Collection]] for this Base Archetype.
By default, all Archetypes that inherit from the Base Archetype produce the same Base Model Type when used as a Factory.
## Changing The Model Type a Child Archetype Produces As A Factory.
[[Archetype Tree Branch|Branches]] of this Archetype Tree produce different types of Models than the Base Model Type.
### Branch Attribute
The Attrubute; [[BranchAttribute]], can be added to a Child Archetype's class if that Child Archetype is a nested class within another class that inherits from the [[Base Archetype]]'s [[Base Model Type]]. This will tell the [[Loader]] that the Archetype with the attribute should produce [[Model]]s of the outer type that extends the Base Model Type.

Ex:
```
public class Item : Model<Item, Item.Type> {
	public class Type : Archetype<Item, Item.Type> {
		...
	}
}

public class Weapon : Item {
	[Branch]
	public class Type : Item.Type {
		...
	}
}
```
In this example, Weapon.Type will produce Weapon Models by default instead of Item Models.

### Overriding The Model Constructor
The model constructor an Archetype uses can either be overriden in the default [[Builder]] the Archetype uses, or in the [[Archetype]] itself.
#### Builder Method 
#### In-Archetype Method
The virtual property [[ModelConstructor]] can be overriden in a Child Archetype class.

Ex:
```
public class Weapon : Item {


	public class Type : Item.Type {
	
      	public override Func<IBuilder<Item>, Item> ModelConstructor
      		=> builder => new Weapon();
		
		...
	}
}
```