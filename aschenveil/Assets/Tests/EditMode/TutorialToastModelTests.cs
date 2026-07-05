using System.Collections.Generic;
using Ashenveil.UI;
using NUnit.Framework;

namespace Ashenveil.Tests.EditMode
{
    public sealed class TutorialToastModelTests
    {
        [Test]
        public void Enqueue_ShowsFirstImmediately()
        {
            var model = new TutorialToastModel(4f);
            string shown = null;
            model.ToastShown += t => shown = t;

            model.Enqueue("move", "Bewege dich mit WASD.");
            Assert.AreEqual("Bewege dich mit WASD.", shown);
            Assert.AreEqual("Bewege dich mit WASD.", model.CurrentText);
        }

        [Test]
        public void Enqueue_DedupesById()
        {
            var model = new TutorialToastModel(4f);
            var shown = new List<string>();
            model.ToastShown += shown.Add;

            model.Enqueue("move", "Bewege dich.");
            model.Enqueue("move", "Bewege dich."); // same id ignored
            model.Tick(5f); // expire first
            model.Enqueue("move", "Bewege dich."); // already shown -> ignored

            Assert.AreEqual(1, shown.Count);
        }

        [Test]
        public void Tick_ExpiresAndPromotesNext()
        {
            var model = new TutorialToastModel(4f);
            var shown = new List<string>();
            model.ToastShown += shown.Add;

            model.Enqueue("a", "Erste");
            model.Enqueue("b", "Zweite");
            Assert.AreEqual("Erste", model.CurrentText);

            model.Tick(4f); // expire first, promote second
            Assert.AreEqual("Zweite", model.CurrentText);
            Assert.AreEqual(new[] { "Erste", "Zweite" }, shown);
        }

        [Test]
        public void Theme_ColorsAndSizesAreValid()
        {
            Assert.Greater(UITheme.FontSizeBody, 0f);
            Assert.Greater(UITheme.FontSizeTitle, UITheme.FontSizeBody);
            Assert.Greater(UITheme.ReferenceResolution.x, 0f);
            Assert.AreEqual(1f, UITheme.HealthFill.a, 1e-4f);
        }
    }
}
