using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libShared
{

    //
    // Zusammenfassung:
    //    RGBA
    public struct SharedColor
    {
        private uint _packedValue;

        /// <summary>
        /// Gets or sets the blue component.
        /// </summary>
        public byte B
        {
            get
            {
                unchecked
                {
                    return (byte)(this.PackedValue >> 16);
                }
            }
            set
            {
                this.PackedValue = (this.PackedValue & 0xff00ffff) | ((uint)value << 16);
            }
        }

        /// <summary>
        /// Gets or sets the green component.
        /// </summary>
        public byte G
        {
            get
            {
                unchecked
                {
                    return (byte)(this.PackedValue >> 8);
                }
            }
            set
            {
                this.PackedValue = (this.PackedValue & 0xffff00ff) | ((uint)value << 8);
            }
        }

        /// <summary>
        /// Gets or sets the red component.
        /// </summary>
        public byte R
        {
            get
            {
                unchecked
                {
                    return (byte)this.PackedValue;
                }
            }
            set
            {
                this.PackedValue = (this.PackedValue & 0xffffff00) | value;
            }
        }

        /// <summary>
        /// Gets or sets the alpha component.
        /// </summary>
        public byte A
        {
            get
            {
                unchecked
                {
                    return (byte)(this.PackedValue >> 24);
                }
            }
            set
            {
                this.PackedValue = (this.PackedValue & 0x00ffffff) | ((uint)value << 24);
            }
        }

        private uint PackedValue
        {
            get
            {
                return _packedValue;
            }

            set
            {
                _packedValue = value;
            }
        }

        public SharedColor(uint packedValue)
        {
            _packedValue = packedValue;
        }
    }

}
