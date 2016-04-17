using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libShared.HardwareNah
{
    /// <summary>
    /// RGB-LED-Settings for color channels and drive current for one LED
    /// </summary>
    public struct RGBValue
    {
        /// <summary>
        /// Defines the Maximum Value that can be set
        /// </summary>
        public const int NormValueWidt = 16;
        public const int MaxValue = Byte.MaxValue;

        private Byte red;
        /// <summary>
        /// Red Value
        /// </summary>
        public Byte Red
        {
            get { return red; }
            set { red = value; }
        }

        private Byte green;
        /// <summary>
        /// Green Value
        /// </summary>
        public Byte Green
        {
            get { return green; }
            set { green = value; }
        }

        private Byte blue;
        /// <summary>
        /// Blue Value
        /// </summary>
        public Byte Blue
        {
            get { return blue; }
            set { blue = value; }
        }

        private Byte intensity;
        /// <summary>
        /// Intensity Value
        /// </summary>
        public Byte Intensity
        {
            get { return intensity; }
            set { intensity = value; }
        }

        /// <summary>
        /// Method to preset all color values
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="i"></param>
        public RGBValue (byte r = 0, byte g = 0, byte b = 0, byte i = 0)
        {
            red = r;
            green = g;
            blue = b;
            intensity = i;

        }

        public static RGBValue operator * (RGBValue value, float factor)
        {
            value.red = (byte)((float)value.red * factor);
            value.green = (byte)((float)value.green * factor);
            value.blue = (byte)((float)value.blue * factor);
            return (value);
        }

        public static RGBValue operator +(RGBValue value1, RGBValue value2)
        {
            value1.red += value2.red;
            value1.green += value2.green;
            value1.blue += value2.blue;
            return (value1);
        }

        public static RGBValue operator -(RGBValue value1, RGBValue value2)
        {
            value1.red -= value2.red;
            value1.green -= value2.green;
            value1.blue -= value2.blue;
            return (value1);
        }
    }

    /// <summary>
    /// Generic RGB-Color-Definitions
    /// </summary>
    public static class RGBDefines
    {
        const Byte MaxValue = Byte.MaxValue;
        public readonly static RGBValue Black = new RGBValue { Intensity = MaxValue, Red = 0, Green = 0, Blue = 0 };
        public readonly static RGBValue Red = new RGBValue { Intensity = MaxValue, Red = MaxValue, Green = 0, Blue = 0 };
        public readonly static RGBValue Green = new RGBValue { Intensity = MaxValue, Red = 0, Green = MaxValue, Blue = 0 };
        public readonly static RGBValue Blue = new RGBValue { Intensity = MaxValue, Red = 0, Green = 0, Blue = MaxValue };
        public readonly static RGBValue Yellow = new RGBValue { Intensity = MaxValue, Red = MaxValue, Green = MaxValue, Blue = 0 };
        public readonly static RGBValue Cyan = new RGBValue { Intensity = MaxValue, Red = 0, Green = MaxValue, Blue = MaxValue };
        public readonly static RGBValue Magenta = new RGBValue { Intensity = MaxValue, Red = MaxValue, Green = 0, Blue = MaxValue };
        public readonly static RGBValue White = new RGBValue { Intensity = MaxValue, Red = MaxValue, Green = MaxValue, Blue = MaxValue };
    }

    /// <summary>
    /// Generates Patterns for LED Stripes
    /// </summary>
    public class PatternGenerator
    {//@todo Doku for Pattern Generator
        public List<Byte[]> Curves;
        public List<eCurveType> CurveTypes;

        public int CurveLenght { get; }
        public int ResolutionFactor { get; }
        public int StripeLenght { get; }

        int curveChangeState = 0;
        int colorSweepState = 0;

        private int colorChangeStep = 10;

        public int ColorChangeStep
        {
            get { return colorChangeStep; }
            set
            {
                if (value < 1)
                {
                    colorChangeStep = 1;
                }
                colorChangeStep = value;
            }
        }

        private int curveChangeStep = 10;

        public int CurveChangeStep
        {
            get { return curveChangeStep; }
            set
            {
                if (value < 1)
                {
                    curveChangeStep = 1;
                }
                curveChangeStep = value;
            }
        }

        public PatternGenerator(int arrayLen, int resFactor)
        {
            StripeLenght = arrayLen;
            CurveLenght = arrayLen * resFactor;
            ResolutionFactor = resFactor;

            Curves = new List<Byte[]>();
            CurveTypes = new List<eCurveType>();

        }

        public void AddCurve(eCurveType curveType)
        {
            Byte[] CurvePrototype = new Byte[CurveLenght * 2];
            Curves.Add(CurvePrototype);
            CurveTypes.Add(curveType);
            // @todo Pattern Generator auf generischen Datentyp umstellen
            switch (curveType)
            {
                case eCurveType.Sine:
                    for (int i = 0; i < CurveLenght; i++)
                    {
                        CurvePrototype[i] = (Byte)((Math.Sin(2 * Math.PI / CurveLenght * i) + 1) / 2 * Byte.MaxValue);
                    }
                    break;
                case eCurveType.Cosine:
                    for (int i = 0; i < CurveLenght; i++)
                    {
                        CurvePrototype[i] = (Byte)((-Math.Cos(2 * Math.PI / CurveLenght * i) + 1) / 2 * Byte.MaxValue);
                    }
                    break;
                case eCurveType.Pulse:
                    Byte[] temp = new Byte[CurveLenght / 10];
                    for (int i = 0; i < temp.Length; i++)
                    {
                        temp[i] = (Byte)((float)1 / (2 * Math.PI) * Math.Exp(-(Math.Pow(3 / temp.Length * i, 2) / 2)) * Byte.MaxValue);
                    }
                    CurvePrototype[CurveLenght / 2] = Byte.MaxValue;
                    for (int i = 1; i < temp.Length; i++)
                    {
                        CurvePrototype[CurveLenght / 2 + i] = temp[i];
                        CurvePrototype[CurveLenght / 2 - i] = temp[i];
                    }
                    break;
                case eCurveType.Triangle:
                    for (int i = 0; i < CurveLenght / 2; i++)
                    {
                        CurvePrototype[i] = (Byte)(Byte.MaxValue / CurveLenght * 2 * i);
                        CurvePrototype[CurveLenght / 2 + i] = (Byte)(Byte.MaxValue - Byte.MaxValue / CurveLenght * 2 * i);
                    }
                    break;
                case eCurveType.Sawtooth:
                    for (int i = 0; i < CurveLenght; i++)
                    {
                        CurvePrototype[i] = (Byte)(Byte.MaxValue / CurveLenght * i);
                    }
                    break;
                default:
                    break;
            }

            /// Prepare second part for optimized moving Window
            for (int i = CurveLenght; i < 2 * CurveLenght; i++)
            {
                CurvePrototype[i] = CurvePrototype[i - CurveLenght];
            }
        }

        int colorChangeCycle = 0;
        int colorCycle = 0;

        RGBValue newRGBrefValue;
        RGBValue oldRGBrefValue;
        public void InitColorChange(RGBValue newrefValue)
        {
            if (colorSweepState == 0)
            {
                colorSweepState = 1;
                oldRGBrefValue = newRGBrefValue;
                newRGBrefValue = newrefValue;
            }
        }

        int curveChangeCycle = 0;
        int CurveIndex = 0;

        int newCurveIndex;
        public void InitCurveChange(int newCurve)
        {
            if (curveChangeState == 0)
            {
                if (newCurve < Curves.Count)
                {
                    curveChangeState = 1;
                    newCurveIndex = newCurve;
                }
            }
        }

        int cycleCount = 0;

        RGBValue currentRGBrefValue;
        public void RefreshData(RGBValue[] rgbStripeHandle)
        {
            if (curveChangeState > 0)
            {
                ExecuteCurveFading();
            }

            if (colorSweepState > 0)
            {
                ExecuteCollorFading();
            }

            for (int idx = 0; idx < StripeLenght; idx++)
            {
                Byte red = (Byte)((int)Curves[CurveIndex][idx + cycleCount] * currentRGBrefValue.Red / RGBValue.MaxValue);
                Byte green = (Byte)((int)Curves[CurveIndex][idx + cycleCount] * currentRGBrefValue.Green / RGBValue.MaxValue);
                Byte blue = (Byte)((int)Curves[CurveIndex][idx + cycleCount] * currentRGBrefValue.Blue / RGBValue.MaxValue);

                rgbStripeHandle[idx] = new RGBValue() { Intensity = currentRGBrefValue.Intensity, Red = red, Green = green, Blue = blue };
            }
            cycleCount++;
            if (cycleCount >= StripeLenght)
            {
                cycleCount = 0;
            }
        }

        private void ExecuteCurveFading()
        {
            switch (curveChangeState)
            {
                case 1:
                    currentRGBrefValue.Intensity = (Byte)((int)oldRGBrefValue.Intensity * (curveChangeStep - curveChangeCycle) / curveChangeStep);
                    curveChangeCycle++;
                    if (curveChangeCycle > curveChangeStep)
                    {
                        curveChangeCycle = 0;
                        curveChangeState++;
                        CurveIndex = newCurveIndex;
                    }
                    break;
                case 2:
                    currentRGBrefValue.Intensity = (Byte)((int)newRGBrefValue.Intensity * curveChangeCycle / curveChangeStep);
                    curveChangeCycle++;
                    if (curveChangeCycle > curveChangeStep)
                    {
                        curveChangeCycle = 0;
                        curveChangeState = 0;
                    }
                    break;
            }
        }

        private void ExecuteCollorFading()
        {
            currentRGBrefValue =
                oldRGBrefValue * ((float)(colorChangeStep - colorChangeCycle) / colorChangeStep) +
                newRGBrefValue * ((float)colorChangeCycle / colorChangeStep);
            colorChangeCycle++;

            if (colorChangeCycle > colorChangeStep)
            {
                colorSweepState = 0;
                colorChangeCycle = 0;
            }
        }

        public enum eCurvequality
        {
            Plain,
            Noisy,
        }

        public enum ePatternMode
        {
            Repetitive,
            Mirrored,
        }

        public enum eCurveType
        {
            Sine,
            Cosine,
            Pulse,
            Triangle,
            Sawtooth,
        }

        public enum eAnimationType
        {
            moveToFirstElement,
            moveToLastElement,
            cumulutativeAddition,
            cumulutativeSubstraction,
        }
    }
}
