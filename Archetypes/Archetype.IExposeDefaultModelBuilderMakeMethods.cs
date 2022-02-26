

using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {
  public partial class Archetype<TModelBase, TArchetypeBase> {

    /// <summary>
    /// Exposes the base set of builder Make functions publicly for ease of access. 
    /// </summary>
    public interface IExposeDefaultModelBuilderMakeMethods {

      /// <summary>
      /// Exposes the base set of builder Make functions that use a builder as their parameter publicly for ease of access. 
      /// </summary>
      public interface WithParamListParameters {

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        /*TModelBase Make(params KeyValuePair<string, object>[] @params)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(@params.AsEnumerable());

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(params KeyValuePair<string, object>[] @params)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(@params.AsEnumerable());*/

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TModelBase Make(IEnumerable<KeyValuePair<string, object>> @params)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(@params);

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(IEnumerable<KeyValuePair<string, object>> @params)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(@params);

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TModelBase Make(params (string key, object value)[] @params)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(@params.AsEnumerable());

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(params (string key, object value)[] @params)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(@params.AsEnumerable());

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TModelBase Make(IEnumerable<(string key, object value)> @params)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(@params);

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(IEnumerable<(string key, object value)> @params)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(@params);

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        /*TModelBase Make(params (IModel.Builder.Param key, object value)[] @params)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(@params.AsEnumerable());

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(params (IModel.Builder.Param key, object value)[] @params)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(@params.AsEnumerable());*/

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TModelBase Make(IEnumerable<(IModel.Builder.Param key, object value)> @params)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(@params);

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(IEnumerable<(IModel.Builder.Param key, object value)> @params)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(@params);

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TModelBase Make(IEnumerable<KeyValuePair<IModel.Builder.Param, object>> @params)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(@params);

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(IEnumerable<KeyValuePair<IModel.Builder.Param, object>> @params)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(@params);

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        /*TModelBase Make(params KeyValuePair<IModel.Builder.Param, object>[] @params)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(@params.AsEnumerable());

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(params KeyValuePair<IModel.Builder.Param, object>[] @params)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(@params.AsEnumerable());*/
      }

      /// <summary>
      /// Exposes the base set of builder Make functions that use a list of Params as their parameters publicly for ease of access. 
      /// </summary>
      public interface WithBuilderParameters {

        /// <summary>
        /// The builder for the base model type of this archetype.
        /// You can override this and add more default props to the return for utility.
        /// </summary>
        IBuilder<TModelBase> MakeDefaultBuilder()
          => (this as Archetype<TModelBase, TArchetypeBase>).MakeDefaultBuilder();

        /// <summary>
        /// The builder for the base model type of this archetype.
        /// You can override this and add more default props to the return for utility.
        /// </summary>
        IBuilder<TModelBase> MakeBuilder(Dictionary<string, object> @params)
          => (this as Archetype<TModelBase, TArchetypeBase>).MakeBuilder(@params);

        /// <summary>
        /// Make a default model from this Archetype
        /// </summary>
        TModelBase Make()
          => (this as Archetype<TModelBase, TArchetypeBase>).Make();

        /// <summary>
        /// Make a default model from this Archetype
        /// </summary>
        TDesiredModel Make<TDesiredModel>()
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>();

        /// <summary>
        /// Make a model by and configuring the default builder.
        /// </summary>
        TModelBase Make(Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(configureBuilder);

        /// <summary>
        /// Make a model by and configuring the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(configureBuilder);


        /// <summary>
        /// Make a model by and configuring the default builder.
        /// </summary>
        TModelBase Make(Action<IModel.Builder> configureBuilder)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(configureBuilder);

        /// <summary>
        /// Make a model by and configuring the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(Action<IModel.Builder> configureBuilder)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(configureBuilder);


        /// <summary>
        /// Make a model by and configuring the default builder.
        /// </summary>
        TModelBase Make(Action<IModel<TModelBase>.Builder> configureBuilder)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(configureBuilder);

        /// <summary>
        /// Make a model by and configuring the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(Action<IModel<TModelBase>.Builder> configureBuilder)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(configureBuilder);

        /// <summary>
        /// Make a model by and configuring the default builder.
        /// </summary>
        TModelBase Make(Func<IBuilder, IBuilder> configureBuilder)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(configureBuilder);

        /// <summary>
        /// Make a model by and configuring the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(Func<IBuilder, IBuilder> configureBuilder)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(configureBuilder);

        /// <summary>
        /// Make a model by and configuring the default builder.
        /// </summary>
        TModelBase Make(Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(configureBuilder);

        /// <summary>
        /// Make a model by and configuring the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(configureBuilder);

        /// <summary>
        /// Make a model from this archetype using a fully qualified builder.
        /// </summary>
        TModelBase Make(IModel<TModelBase>.Builder builder)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(builder);

        /// <summary>
        /// Make a model from this archetype using a fully qualified builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(IModel<TModelBase>.Builder builder)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(builder);

        /// <summary>
        /// Make a model from this archetype using a fully qualified builder.
        /// </summary>
        TModelBase Make(IBuilder<TModelBase> builder)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(builder);

        /// <summary>
        /// Make a model from this archetype using a fully qualified builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(IBuilder<TModelBase> builder)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(builder);
      }

      /// <summary>
      /// Exposes the entire base set of Make functions publicly for ease of access. 
      /// </summary>
      public interface Fully : WithBuilderParameters, WithParamListParameters {

        /// <summary>
        /// Make a default model from this Archetype
        /// </summary>
       new TModelBase Make()
          => (this as Archetype<TModelBase, TArchetypeBase>).Make();

        /// <summary>
        /// Make a default model from this Archetype
        /// </summary>
        new TDesiredModel Make<TDesiredModel>() 
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>();
      }
    }
  }

  /// <summary>
  /// Public make extensions for archeytpes that implement Archetype.IExposePublicMakers.Fully
  /// </summary>
  public static class ArchetypePublicFullMakers {

    /// <summary>
    /// Exposes the interface for any public model builder Make functions for this archetype.
    /// </summary>
    public static Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.Fully DefaultModelBuilders<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.Fully @this)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this;

    /// <summary>
    /// Make a default model from this Archetype
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.Fully @this)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make();

    /// <summary>
    /// Make a default model from this Archetype of the desired sub-type
    /// </summary>
    public static TModelBase Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.Fully @this)
      where TDesiredModel : class, TModelBase
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>();

    /// <summary>
    /// Make a default model from this Archetype of the desired sub-type.
    /// This returns the same value passed to the out parameter, this is mainly just here to help you avoid entering all 3 generic types when you want to use Make in a simple way.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.Fully @this, out TDesiredModel model)
      where TDesiredModel : TModelBase
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>();
  }

  /// <summary>
  /// Public make extensions for archeytpes that implement Archetype.IExposePublicMakers.WithBuilderParameters or ..Fully
  /// </summary>
  public static class ArchetypePublicBuilderMakers {

    /// <summary>
    /// The builder for the base model type of this archetype.
    /// You can override this and add more default props to the return for utility.
    /// </summary>
    public static IBuilder<TModelBase> MakeDefaultBuilder<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.MakeDefaultBuilder();

    /// <summary>
    /// The builder for the base model type of this archetype.
    /// You can override this and add more default props to the return for utility.
    /// </summary>
    public static IBuilder<TModelBase> MakeBuilder<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, Dictionary<string, object> @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.MakeBuilder(@params);

    /// <summary>
    /// Gets the exposed model builder Make functions for this archetype.
    /// </summary>
    public static Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters DefaultModelBuilders<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this;

    /// <summary>
    /// Make a default model from this Archetype
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make();

    /// <summary>
    /// Make a default model from this Archetype of the desired sub-type
    /// </summary>
    public static TModelBase Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this)
      where TDesiredModel : class, TModelBase
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>();

    /// <summary>
    /// Make a default model from this Archetype of the desired sub-type.
    /// This returns the same value passed to the out parameter, this is mainly just here to help you avoid entering all 3 generic types when you want to use Make in a simple way.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, out TDesiredModel model)
      where TDesiredModel : class, TModelBase
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>();

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      => @this.Make(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(configureBuilder);

    /// <summary>
    /// Make a default model from this Archetype of the desired sub-type.
    /// This returns the same value passed to the out parameter, this is mainly just here to help you avoid entering all 3 generic types when you want to use Make in a simple way.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, out TDesiredModel model, Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(configureBuilder);


    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, Action<IModel.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, Action<IModel.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// This returns the same value passed to the out parameter, this is mainly just here to help you avoid entering all 3 generic types when you want to use Make in a simple way.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, out TDesiredModel model, Action<IModel.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, Action<IModel<TModelBase>.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, Action<IModel<TModelBase>.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(configureBuilder);


    /// <summary>
    /// Make a model by and configuring the default builder.
    /// This returns the same value passed to the out parameter, this is mainly just here to help you avoid entering all 3 generic types when you want to use Make in a simple way.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, out TDesiredModel model, Action<IModel<TModelBase>.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, Func<IBuilder, IBuilder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, Func<IBuilder, IBuilder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// This returns the same value passed to the out parameter, this is mainly just here to help you avoid entering all 3 generic types when you want to use Make in a simple way.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, out TDesiredModel model, Func<IBuilder, IBuilder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(configureBuilder);

    /// <summary>
    /// Make a model from this archetype by passing down and updating a default builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(configureBuilder);

    /// <summary>
    /// Make a model from this archetype by passing down and updating a default builder.
    /// </summary>
    public static TDesiredModel Make<TModelBase, TArchetypeBase, TDesiredModel>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(configureBuilder);

    /// <summary>
    /// Make a model from this archetype by passing down and updating a default builder.
    /// This returns the same value passed to the out parameter, this is mainly just here to help you avoid entering all 3 generic types when you want to use Make in a simple way.
    /// </summary>
    public static TDesiredModel Make<TModelBase, TArchetypeBase, TDesiredModel>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, out TDesiredModel model, Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(configureBuilder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, IModel<TModelBase>.Builder builder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(builder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, IModel<TModelBase>.Builder builder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(builder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// This returns the same value passed to the out parameter, this is mainly just here to help you avoid entering all 3 generic types when you want to use Make in a simple way.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, out TDesiredModel model, IModel<TModelBase>.Builder builder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(builder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, IBuilder<TModelBase> builder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(builder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, IBuilder<TModelBase> builder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(builder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// This returns the same value passed to the out parameter, this is mainly just here to help you avoid entering all 3 generic types when you want to use Make in a simple way.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters @this, out TDesiredModel model, IBuilder<TModelBase> builder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(builder);
  }

  /// <summary>
  /// Public make extensions for archeytpes that implement Archetype.IExposePublicMakers.WithParamListParameters or ..Fully
  /// </summary>
  public static class ArchetypePublicParameterListMakers {

    /// <summary>
    /// Gets the exposed model builder Make functions for this archetype.
    /// </summary>
    public static Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters DefaultModelBuilders<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this;

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// This does by default for models.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, IEnumerable<KeyValuePair<string, object>> @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, IEnumerable<KeyValuePair<string, object>> @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, out TDesiredModel model, IEnumerable<KeyValuePair<string, object>> @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// This does by default for models.
    /// </summary>
    /*public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, params KeyValuePair<string, object>[] @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(@params.AsEnumerable());

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, params KeyValuePair<string, object>[] @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(@params.AsEnumerable());

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, out TDesiredModel model, params KeyValuePair<string, object>[] @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(@params.AsEnumerable());*/

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, IEnumerable<(string key, object value)> @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, IEnumerable<(string key, object value)> @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, out TDesiredModel model, IEnumerable<(string key, object value)> @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, params (string key, object value)[] @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(@params.AsEnumerable());

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, params (string key, object value)[] @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(@params.AsEnumerable());

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, out TDesiredModel model, params (string key, object value)[] @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(@params.AsEnumerable());

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, IEnumerable<(IModel.Builder.Param key, object value)> @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, IEnumerable<(IModel.Builder.Param key, object value)> @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, out TDesiredModel model, IEnumerable<(IModel.Builder.Param key, object value)> @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    /*public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, params (IModel.Builder.Param key, object value)[] @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(@params.AsEnumerable());

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, params (IModel.Builder.Param key, object value)[] @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(@params.AsEnumerable());

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, out TDesiredModel model, params (IModel.Builder.Param key, object value)[] @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(@params.AsEnumerable());*/

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, IEnumerable<KeyValuePair<IModel.Builder.Param, object>> @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, IEnumerable<KeyValuePair<IModel.Builder.Param, object>> @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, out TDesiredModel model, IEnumerable<KeyValuePair<IModel.Builder.Param, object>> @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    /*public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, params KeyValuePair<IModel.Builder.Param, object>[] @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, params KeyValuePair<IModel.Builder.Param, object>[] @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, out TDesiredModel model, params KeyValuePair<IModel.Builder.Param, object>[] @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(@params);*/
  }
}