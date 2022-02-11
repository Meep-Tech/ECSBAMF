

using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {
  public partial class Archetype<TModelBase, TArchetypeBase> {

    /// <summary>
    /// Exposes the base set of builder Make functions publicly for ease of access. 
    /// </summary>
    public interface IExposePublicMakers {

      /// <summary>
      /// Exposes the base set of builder Make functions that use a builder as their parameter publicly for ease of access. 
      /// </summary>
      public interface WithBuilderParameters {

        /// <summary>
        /// Make a default model from this Archetype
        /// </summary>
        TModelBase Make()
          => (this as Archetype<TModelBase, TArchetypeBase>).Make();

        /// <summary>
        /// Make a default model from this Archetype
        /// </summary>
        TDesiredModel Make<TDesiredModel>()
          where TDesiredModel : class, TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>();
      }

      /// <summary>
      /// Exposes the base set of builder Make functions that use a list of Params as their parameters publicly for ease of access. 
      /// </summary>
      public interface WithBuilderBasedParameters {


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

        /// <summary>
        /// Make a model from this archetype using a fully qualified builder.
        /// </summary>
        TDesiredModel MakeAs<TDesiredModel>(Action<IModel.Builder> configureBuilder, out TDesiredModel model)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).MakeAs(configureBuilder, out model);

        /// <summary>
        /// Make a model from this archetype using a fully qualified builder.
        /// </summary>
        TDesiredModel MakeAs<TDesiredModel>(IModel<TModelBase>.Builder builder, out TDesiredModel model)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).MakeAs(builder, out model);

      }

      /// <summary>
      /// Exposes the entire base set of Make functions publicly for ease of access. 
      /// </summary>
      public interface Fully : WithBuilderBasedParameters, WithBuilderParameters {

        /// <summary>
        /// Make a default model from this Archetype
        /// </summary>
       new TModelBase Make()
          => (this as Archetype<TModelBase, TArchetypeBase>).Make();

        /// <summary>
        /// Make a default model from this Archetype
        /// </summary>
        new TDesiredModel Make<TDesiredModel>() 
          where TDesiredModel : class, TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>();

        /*/// <summary>
        /// Make a default model from this Archetype
        /// </summary>
        TDesiredModel MakeAs<TDesiredModel>(out TDesiredModel model) 
          where TDesiredModel : class, TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).MakeAs(out model);*/
      }
    }
  }

  /// <summary>
  /// Public make extensions for archeytpes that implement Archetype.IExposePublicMakers.Fully
  /// </summary>
  public static class ArchetypePublicFullMakers {

    /// <summary>
    /// Make a default model from this Archetype
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.Fully @this)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make();

    /// <summary>
    /// Make a default model from this Archetype of the desired sub-type
    /// </summary>
    public static TModelBase Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.Fully @this)
      where TDesiredModel : class, TModelBase
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>();

    /// <summary>
    /// Make a default model from this Archetype of the desired sub-type
    /// </summary>
    /*public static TModelBase MakeAs<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.Fully @this, out TDesiredModel model)
      where TDesiredModel : class, TModelBase
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.MakeAs(out model);*/
  }

  /// <summary>
  /// Public make extensions for archeytpes that implement Archetype.IExposePublicMakers.WithBuilderParameters or ..Fully
  /// </summary>
  public static class ArchetypePublicBuilderMakers {

    /// <summary>
    /// Make a default model from this Archetype
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make();

    /// <summary>
    /// Make a default model from this Archetype of the desired sub-type
    /// </summary>
    public static TModelBase Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.Fully @this)
      where TDesiredModel : class, TModelBase
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>();

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      => @this.Make(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(configureBuilder);


    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, Action<IModel.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, Action<IModel.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, Action<IModel<TModelBase>.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TModelBase Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, Action<IModel<TModelBase>.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, Func<IBuilder, IBuilder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TModelBase Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, Func<IBuilder, IBuilder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(configureBuilder);

    /// <summary>
    /// Make a model from this archetype by passing down and updating a default builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(configureBuilder);

    /// <summary>
    /// Make a model from this archetype by passing down and updating a default builder.
    /// </summary>
    public static TDesiredModel Make<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(configureBuilder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, IModel<TModelBase>.Builder builder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(builder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, IModel<TModelBase>.Builder builder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(builder);


    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, IBuilder<TModelBase> builder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(builder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, IBuilder<TModelBase> builder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(builder);

    /// <summary>
    /// Make a model that requires an object based builder:
    /// </summary>
    public static TDesiredModel MakeAs<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, Action<IModel.Builder> configureBuilder, out TDesiredModel model)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : class, TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.MakeAs(configureBuilder, out model);

    /// <summary>
    /// Make a model that requires a struct based builder
    /// </summary>
    public static TDesiredModel MakeAs<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder, out TDesiredModel model)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : class, TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.MakeAs(configureBuilder, out model);

    /// <summary>
    /// Make a model from this archetype by passing down and updating a default builder.
    /// </summary>
    public static TDesiredModel MakeAs<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder, out TDesiredModel model)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : class, TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.MakeAs(configureBuilder, out model);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    public static TDesiredModel MakeAs<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, IModel<TModelBase>.Builder builder, out TDesiredModel model)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : class, TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.MakeAs(builder, out model);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    public static TDesiredModel MakeAs<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, IBuilder<TModelBase> builder, out TDesiredModel model)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : class, TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.MakeAs<TDesiredModel>(builder, out model);

    /// <summary>
    /// Make a model from this archetype by passing down and updating a default builder.
    /// </summary>
    public static bool TryToMakeAs<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder, out TDesiredModel model)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : class, TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.TryToMakeAs(configureBuilder, out model);

    /// <summary>
    /// Make a model that requires an object based builder:
    /// </summary>
    public static bool TryToMakeAs<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, Action<IModel.Builder> configureBuilder, out TDesiredModel model)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : class, TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.TryToMakeAs(configureBuilder, out model);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    public static bool TryToMakeAs<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, IModel<TModelBase>.Builder builder, out TDesiredModel model)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : class, TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.TryToMakeAs(builder, out model);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    public static bool TryToMakeAs<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, IBuilder<TModelBase> builder, out TDesiredModel model)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : class, TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.TryToMakeAs(builder, out model);

    /// <summary>
    /// Make a model that requires a struct based builder"
    /// </summary>
    public static bool TryToMakeAs<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this Archetype<TModelBase, TArchetypeBase>.IExposePublicMakers.WithBuilderBasedParameters @this, Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder, out TDesiredModel model)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : class, TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.TryToMakeAs(configureBuilder, out model);
  }

  /// <summary>
  /// Public make extensions for archeytpes that implement Archetype.IExposePublicMakers.WithParamListParameters or ..Fully
  /// </summary>
  public static class ArchetypePublicParameterListMakers {

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// This does by default for models.
    /// </summary>
    /// <returns></returns>
    public static TModelBase Make<TArchetype, TModelBase, TArchetypeBase>(this TArchetype @this, IEnumerable<KeyValuePair<string, object>> @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TModelBase Make<TArchetype, TModelBase, TArchetypeBase>(this TArchetype @this, IEnumerable<(string key, object value)> @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TModelBase Make<TArchetype, TModelBase, TArchetypeBase>(this TArchetype @this, params (string key, object value)[] @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TModelBase Make<TArchetype, TModelBase, TArchetypeBase>(this TArchetype @this, IEnumerable<(IModel.Builder.Param key, object value)> @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TModelBase Make<TArchetype, TModelBase, TArchetypeBase>(this TArchetype @this, params (IModel.Builder.Param key, object value)[] @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TModelBase Make<TArchetype, TModelBase, TArchetypeBase>(this TArchetype @this, IEnumerable<KeyValuePair<IModel.Builder.Param, object>> @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TModelBase Make<TArchetype, TModelBase, TArchetypeBase>(this TArchetype @this, params KeyValuePair<IModel.Builder.Param, object>[] @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    public static TModelBase Make<TArchetype, TModelBase, TArchetypeBase>(this TArchetype @this, params KeyValuePair<string, object>[] @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.Make(@params.AsEnumerable());

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    public static TDesiredModel Make<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this TArchetype @this, params (string key, object value)[] @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.Make<TDesiredModel>(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    public static TDesiredModel Make<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this TArchetype @this, IEnumerable<KeyValuePair<string, object>> @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.Make<TDesiredModel>(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    public static TDesiredModel Make<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this TArchetype @this, params KeyValuePair<string, object>[] @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.Make<TDesiredModel>(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    public static void MakeAs<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this TArchetype @this, out TDesiredModel model, params (string key, object value)[] @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : class, TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.MakeAs(@params, out model);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    public static void MakeAs<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this TArchetype @this, IEnumerable<KeyValuePair<string, object>> @params, out TDesiredModel model)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : class, TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.MakeAs(@params, out model);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    public static void MakeAs<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this TArchetype @this, out TDesiredModel model, params KeyValuePair<string, object>[] @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : class, TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.MakeAs(out model, @params);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    public static bool TryToMakeAs<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this TArchetype @this, out TDesiredModel model, params (string key, object value)[] @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : class, TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.MakeAs(@params, out model) is not null;

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    public static bool TryToMakeAs<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this TArchetype @this, IEnumerable<KeyValuePair<string, object>> @params, out TDesiredModel model)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : class, TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.MakeAs(@params, out model) is not null;

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    public static bool TryToMakeAs<TArchetype, TModelBase, TArchetypeBase, TDesiredModel>(this TArchetype @this, out TDesiredModel model, params KeyValuePair<string, object>[] @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : class, TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      where TArchetype : TArchetypeBase, Archetype.IExposePublicMakers.WithParamListParameters
        => @this.MakeAs(out model, @params) is not null;
  }
}