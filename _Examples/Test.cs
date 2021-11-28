using System;
using System.Collections.Generic;
using System.Text;

namespace Meep.Tech.Data.Examples {

  public class Item : Model<Item, Item.Type> {
    public class Type : Archetype<Item, Type> {

      public new static Identity Id {
        get;
      }

      public Type() : base(Id) {

      }
    }

    public void main(Item item) {
      Item.Type itemType;

      itemType = Item.Types.Get(Item.Type.Id);
      itemType = Item.Types.ById[Item.Type.Id];
      itemType = Item.Types.Get<Item.Type>();
      itemType = Item.Types.Get(typeof(Item.Type));
      itemType = Item.Type.Collection.Get(Item.Type.Id);
      itemType = Archetypes<Item.Type>.Archetype;
      itemType = Archetypes<Item.Type>.Instance;
      itemType = Archetypes<Item.Type>._;
      itemType = Archetypes.All.Get(Item.Type.Id) as Item.Type;
      itemType = Archetypes.All.ById[Item.Type.Id] as Item.Type;
      itemType = typeof(Item.Type).AsArchetype<Item.Type>();
      itemType = typeof(Item.Type).AsArchetype() as Item.Type;
      itemType = Item.Type.Id.Archetype as Item.Type;

      itemType.Make(new Model<Item>.Builder(itemType) {
        {"color", "red" }
      });
      Item.Make(itemType, (Builder builder) => {
        builder.set("color", "red");
      });
      Item.Types.Get<Item.Type>().Make((IBuilder<Item> builder) => {
        builder.set("color", "red");
        return builder;
      });
      Item.Types.Get<Item.Type>().Make((IBuilder builder) => builder);
      Item.Types.Get<Item.Type>().Make((Model<Item>.Builder builder) => builder);
      Item.Types.Get<Item.Type>().Make(("color", "red"));
      Item.Types.Get<Item.Type>().Make(new KeyValuePair<string, object>("color", "red"));
      Item.Types.Get<Item.Type>().Make(
        ("color", "red"),
        ("count", 3)
      );
      itemType.Make(builder => {
        builder.set("color", "red");
        builder.set("count", 3);
      });

      var color = Components<Color>.BuilderFactory.Make(("color", "red"));
      var color1 = Components<Color>.BuilderFactory.Make(new KeyValuePair<string, object>("color", "red"));
      var color2 = Components<Color>.BuilderFactory.Make(new KeyValuePair<Model.Builder.Param, object>(Color.ColorParam, "red"));
      var color3 = Components<Color>.BuilderFactory.Make((Color.ColorParam, "red"));

      var color4 = Components<Color>.BuilderFactory.Make((IBuilder<Color> builder) => {
        builder.set(Color.ColorParam, "red");

        return builder;
      });

      var color5 = Components<Color>.BuilderFactory.Make((IBuilder<Color> builder) => {
        builder.set("color", "red");

        return builder;
      });

      var builder = new IComponent<Color>.Builder();
      builder.set(Color.ColorParam, "blue");
      var color6 = Components<Color>.BuilderFactory.Make(builder);
    }
  }

  public struct Color : Model.IComponent<Color> {

    public static Model.Builder.Param ColorParam {

      get;
    } = new Model.Builder.Param("color", typeof(string));

    public string color {
      get;
      private set;
    }

    Color(IBuilder builder) {
      color = builder.get<string>("color");
    }

    // You could do this instead of the default ctor if you want:
    static Color() {
      Models<Color>.BuilderFactory.NewBuilderConstructor =
        type => {
          var builder = new Model<Color>.Builder(type) {
            initializeModel = builder => new Color(),
            configureModel = (builder, color) => {
              color.color = builder.get<string>("color");
              return color;
            },
          };

          return builder;
        };
    }
  }
}
