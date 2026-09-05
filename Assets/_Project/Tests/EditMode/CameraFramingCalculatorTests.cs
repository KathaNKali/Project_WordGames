using NUnit.Framework;
using UnityEngine;
using StickersOut.Core;

namespace StickersOut.Tests.EditMode
{
    public class CameraFramingCalculatorTests
    {
        [Test]
        public void SquareGrid_SquareAspect_FitsHeightAndWidthEqually()
        {
            // grid 8x8, margin 1 -> target bounds 10x10, aspect 1:1
            CameraFraming framing = CameraFramingCalculator.Calculate(
                gridWorldWidth: 8f, gridWorldHeight: 8f, margin: 1f, cameraAspect: 1f);

            Assert.AreEqual(5f, framing.OrthographicSize, 0.0001f);
        }

        [Test]
        public void TallGrid_WideAspect_ConstrainedByWidth()
        {
            // grid 4x20, margin 0, aspect 2:1 (wide viewport)
            // targetWidth=4, targetHeight=20
            // sizeToFitHeight = 10
            // sizeToFitWidth = (4/2)/2 = 1
            // max -> 10 (height-constrained even though aspect is wide, because grid is very tall)
            CameraFraming framing = CameraFramingCalculator.Calculate(
                gridWorldWidth: 4f, gridWorldHeight: 20f, margin: 0f, cameraAspect: 2f);

            Assert.AreEqual(10f, framing.OrthographicSize, 0.0001f);
        }

        [Test]
        public void WideGrid_NarrowAspect_ConstrainedByWidth()
        {
            // grid 20x4, margin 0, aspect 0.5 (portrait/narrow viewport)
            // targetWidth=20, targetHeight=4
            // sizeToFitHeight = 2
            // sizeToFitWidth = (20/0.5)/2 = 20
            // max -> 20 (width-constrained on a narrow viewport)
            CameraFraming framing = CameraFramingCalculator.Calculate(
                gridWorldWidth: 20f, gridWorldHeight: 4f, margin: 0f, cameraAspect: 0.5f);

            Assert.AreEqual(20f, framing.OrthographicSize, 0.0001f);
        }

        [Test]
        public void Margin_IncreasesRequiredOrthographicSize()
        {
            CameraFraming noMargin = CameraFramingCalculator.Calculate(
                gridWorldWidth: 6f, gridWorldHeight: 6f, margin: 0f, cameraAspect: 1f);
            CameraFraming withMargin = CameraFramingCalculator.Calculate(
                gridWorldWidth: 6f, gridWorldHeight: 6f, margin: 2f, cameraAspect: 1f);

            Assert.Greater(withMargin.OrthographicSize, noMargin.OrthographicSize);
            Assert.AreEqual(3f, noMargin.OrthographicSize, 0.0001f);
            Assert.AreEqual(5f, withMargin.OrthographicSize, 0.0001f);
        }

        [Test]
        public void Center_MatchesProvidedGridCenter()
        {
            var center = new Vector2(1.5f, -2f);
            CameraFraming framing = CameraFramingCalculator.Calculate(
                gridWorldWidth: 4f, gridWorldHeight: 4f, margin: 0.5f, cameraAspect: 1f, gridCenter: center);

            Assert.AreEqual(center, framing.Center);
        }

        [Test]
        public void InvalidArguments_Throw()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                CameraFramingCalculator.Calculate(0f, 5f, 1f, 1f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                CameraFramingCalculator.Calculate(5f, 0f, 1f, 1f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                CameraFramingCalculator.Calculate(5f, 5f, -1f, 1f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                CameraFramingCalculator.Calculate(5f, 5f, 1f, 0f));
        }
    }
}
