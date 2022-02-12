using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Meep.Tech.Data {

  public abstract partial class Archetype<TModelBase, TArchetypeBase>
    : Archetype, IFactory
    where TModelBase : IModel<TModelBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
  {

    /// <summary>
    /// A root archetype with IExposeDefaultModelBuilderMakeMethods.Fully implemented from the start.
    /// </summary>
    public abstract partial class WithAllDefaultModelBuilders
      : WithDefaultBuilderBasedModelBuilders,
        IExposeDefaultModelBuilderMakeMethods.Fully 
    {
      protected WithAllDefaultModelBuilders(Archetype.Identity id, Collection collection = null) 
        : base(id, collection) {}

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      public new TDesiredModel Make<TDesiredModel>()
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>();

      #region Copied from WithDefaultParamBasedModelBuilders

      /// <summary>
      /// Make a model from this archetype using a set of params to populate the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(params KeyValuePair<string, object>[] @params)
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>(@params.AsEnumerable());

      /// <summary>
      /// Make a model from this archetype using a set of params to populate the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(params (string key, object value)[] @params)
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>(@params.AsEnumerable());

      /// <summary>
      /// Make a model from this archetype using a set of params to populate the default builder.
      /// </summary>
      public TDesiredModel Make<TDesiredModel>(params (IModel.Builder.Param key, object value)[] @params)
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>(@params.AsEnumerable());

      /// <summary>
      /// Make a model from this archetype using a set of params to populate the default builder.
      /// </summary>
      public TDesiredModel Make<TDesiredModel>(params KeyValuePair<IModel.Builder.Param, object>[] @params)
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>(@params.AsEnumerable());

      /// <summary>
      /// Make a model from this archetype using a set of params to populate the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(IEnumerable<KeyValuePair<string, object>> @params)
        where TDesiredModel : TModelBase
            => base.Make<TDesiredModel>(@params);

      /// <summary>
      /// Make a model from this archetype using a set of params to populate the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(IEnumerable<(string key, object value)> @params)
        where TDesiredModel : TModelBase
            => base.Make<TDesiredModel>(@params);

      /// <summary>
      /// Make a model from this archetype using a set of params to populate the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(IEnumerable<(IModel.Builder.Param key, object value)> @params)
        where TDesiredModel : TModelBase
            => base.Make<TDesiredModel>(@params);

      /// <summary>
      /// Make a model from this archetype using a set of params to populate the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(IEnumerable<KeyValuePair<IModel.Builder.Param, object>> @params)
        where TDesiredModel : TModelBase
            => base.Make<TDesiredModel>(@params);

      #endregion
    }

    /// <summary>
    /// A root archetype with IExposeDefaultModelBuilderMakeMethods.Fully implemented from the start.
    /// </summary>
    public abstract partial class WithDefaultParamBasedModelBuilders
      : Archetype<TModelBase, TArchetypeBase>,
        IExposeDefaultModelBuilderMakeMethods.WithBuilderParameters
    {
      protected WithDefaultParamBasedModelBuilders(Archetype.Identity id, Collection collection = null) 
        : base(id, collection) {}

      /// <summary>
      /// Make a model from this archetype using a set of params to populate the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(params KeyValuePair<string, object>[] @params)
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>(@params.AsEnumerable());

      /// <summary>
      /// Make a model from this archetype using a set of params to populate the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(params (string key, object value)[] @params)
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>(@params.AsEnumerable());

      /// <summary>
      /// Make a model from this archetype using a set of params to populate the default builder.
      /// </summary>
      public TDesiredModel Make<TDesiredModel>(params (IModel.Builder.Param key, object value)[] @params)
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>(@params.AsEnumerable());

      /// <summary>
      /// Make a model from this archetype using a set of params to populate the default builder.
      /// </summary>
      public TDesiredModel Make<TDesiredModel>(params KeyValuePair<IModel.Builder.Param, object>[] @params)
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>(@params.AsEnumerable());

      /// <summary>
      /// Make a model from this archetype using a set of params to populate the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(IEnumerable<KeyValuePair<string, object>> @params)
        where TDesiredModel : TModelBase
            => base.Make<TDesiredModel>(@params);

      /// <summary>
      /// Make a model from this archetype using a set of params to populate the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(IEnumerable<(string key, object value)> @params)
        where TDesiredModel : TModelBase
            => base.Make<TDesiredModel>(@params);

      /// <summary>
      /// Make a model from this archetype using a set of params to populate the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(IEnumerable<(IModel.Builder.Param key, object value)> @params)
        where TDesiredModel : TModelBase
            => base.Make<TDesiredModel>(@params);

      /// <summary>
      /// Make a model from this archetype using a set of params to populate the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(IEnumerable<KeyValuePair<IModel.Builder.Param, object>> @params)
        where TDesiredModel : TModelBase
            => base.Make<TDesiredModel>(@params);
    }

    /// <summary>
    /// A root archetype with IExposeDefaultModelBuilderMakeMethods.Fully implemented from the start.
    /// </summary>
    public abstract partial class WithDefaultBuilderBasedModelBuilders
      : Archetype<TModelBase, TArchetypeBase>,
        IExposeDefaultModelBuilderMakeMethods.WithParamListParameters
    {
      protected WithDefaultBuilderBasedModelBuilders(Archetype.Identity id, Collection collection = null) 
        : base(id, collection) {
      }

      /// <summary>
      /// Make a default model from this Archetype
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>()
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>();

      /// <summary>
      /// Make a model by and configuring the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>(configureBuilder);

      /// <summary>
      /// Make a model by and configuring the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(Action<IModel.Builder> configureBuilder)
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>(configureBuilder);

      /// <summary>
      /// Make a model by and configuring the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(Action<IModel<TModelBase>.Builder> configureBuilder)
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>(configureBuilder);

      /// <summary>
      /// Make a model by and configuring the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(Func<IBuilder, IBuilder> configureBuilder)
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>(configureBuilder);

      /// <summary>
      /// Make a model by and configuring the default builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>(configureBuilder);

      /// <summary>
      /// Make a model from this archetype using a fully qualified builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(IModel<TModelBase>.Builder builder)
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>(builder);

      /// <summary>
      /// Make a model from this archetype using a fully qualified builder.
      /// </summary>
      new public TDesiredModel Make<TDesiredModel>(IBuilder<TModelBase> builder)
        where TDesiredModel : TModelBase
          => base.Make<TDesiredModel>(builder);
    }
  }
}