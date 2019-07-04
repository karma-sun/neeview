// from http://blog.okazuki.jp/entry/20110507/1304738683

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Windows
{
    using System.Windows;
    using System.Windows.Interactivity;

    /// <summary>
    /// StyleでBehaviorを設定するために使用するコレクション
    /// </summary>
    public class StyleBehaviorCollection : FreezableCollection<Behavior>
    {
        protected override Freezable CreateInstanceCore()
        {
            return new StyleBehaviorCollection();
        }
    }

    /// <summary>
    /// StyleでTriggerを設定するためのコレクション
    /// </summary>
    public class StyleTriggerCollection : FreezableCollection<System.Windows.Interactivity.TriggerBase>
    {
        protected override Freezable CreateInstanceCore()
        {
            return new StyleTriggerCollection();
        }
    }

    /// <summary>
    /// Style内でBehaviorを設定するのに使用します。
    /// </summary>
    public static class StyleInteraction
    {
        /// <summary>
        /// Style内でBehaviorを設定します。
        /// </summary>
        public static readonly DependencyProperty BehaviorsProperty =
            DependencyProperty.RegisterAttached(
                "Behaviors",
                typeof(StyleBehaviorCollection),
                typeof(StyleInteraction),
                new PropertyMetadata((sender, e) =>
                {
                    if (e.NewValue == e.OldValue)
                    {
                        return;
                    }

                    var styleBehaviors = e.NewValue as StyleBehaviorCollection;
                    if (styleBehaviors == null)
                    {
                        return;
                    }

                    // 同じビヘイビアを複数個所に設定できないので、クローンを追加する
                    var behaviors = Interaction.GetBehaviors(sender);
                    foreach (var styleBehavior in styleBehaviors)
                    {
                        behaviors.Add((Behavior)styleBehavior.Clone());
                    }
                }));

        /// <summary>
        /// Style内でTriggerを設定します。
        /// </summary>
        public static readonly DependencyProperty TriggersProperty =
           DependencyProperty.RegisterAttached(
               "Triggers",
               typeof(StyleTriggerCollection),
               typeof(StyleInteraction),
               new PropertyMetadata((sender, e) =>
               {
                   if (e.NewValue == e.OldValue)
                   {
                       return;
                   }

                   var styleTriggers = e.NewValue as StyleTriggerCollection;
                   if (styleTriggers == null)
                   {
                       return;
                   }

                   var triggers = Interaction.GetTriggers(sender);
                   foreach (var styleTrigger in styleTriggers)
                   {
                       // TriggerとActionのCloneを作成して追加する
                       var clone = (System.Windows.Interactivity.TriggerBase)styleTrigger.Clone();
                       foreach (var action in styleTrigger.Actions)
                       {
                           clone.Actions.Add((System.Windows.Interactivity.TriggerAction)action.Clone());
                       }

                       triggers.Add(clone);
                   }
               }));

        /// <summary>
        /// Style内でBehaviorを設定するコレクションを取得します。
        /// </summary>
        /// <param name="obj">対象のオブジェクト</param>
        /// <returns>Behaviorを設定するコレクション</returns>
        public static StyleBehaviorCollection GetBehaviors(DependencyObject obj)
        {
            return (StyleBehaviorCollection)obj.GetValue(BehaviorsProperty);
        }

        /// <summary>
        /// Style内でBehaviorを設定するコレクションを設定します。
        /// </summary>
        /// <param name="obj">対象のオブジェクト</param>
        /// <param name="value">設定するコレクション</param>
        public static void SetBehaviors(DependencyObject obj, StyleBehaviorCollection value)
        {
            obj.SetValue(BehaviorsProperty, value);
        }

        /// <summary>
        /// Style内でTriggerを設定するコレクションを取得します。
        /// </summary>
        /// <param name="obj">対象のオブジェクト</param>
        /// <returns>Triggerを設定するコレクション</returns>
        public static StyleTriggerCollection GetTriggers(DependencyObject obj)
        {
            return (StyleTriggerCollection)obj.GetValue(BehaviorsProperty);
        }

        /// <summary>
        /// Style内でTriggerを設定するコレクションを設定します。
        /// </summary>
        /// <param name="obj">対象のオブジェクト</param>
        /// <param name="value">設定するコレクション</param>
        public static void SetTriggers(DependencyObject obj, StyleTriggerCollection value)
        {
            obj.SetValue(BehaviorsProperty, value);
        }
    }
}
