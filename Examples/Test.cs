using System;
using System.Collections.Generic;
using System.Text;

namespace Meep.Tech.Data.Examples {

  internal class Item : Model<Item> {

    static Item() {

      Models<Item>.BuilderFactory.NewBuilderConstructor 
        = type => new Model<Item>.Builder(type);
    }

    public void main(Item item) {
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
