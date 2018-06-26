﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClassicUO.Assets
{
    public static class Art
    {
        private static UOFile _file;

        public static void Load()
        {
            string filepath = Path.Combine(FileManager.UoFolderPath, "artLegacyMUL.uop");

            if (File.Exists(filepath))
                _file = new UOFileUop(filepath, ".tga", 0x10000);
            else
            {
                filepath = Path.Combine(FileManager.UoFolderPath, "art.mul");
                string idxpath = Path.Combine(FileManager.UoFolderPath, "artidx.mul");
                if (File.Exists(filepath) && File.Exists(idxpath))
                    _file = new UOFileMul(filepath, idxpath, 0x10000);
            }
        }

       // private static readonly Dictionary<ushort, ushort[]> _pixels = new Dictionary<ushort, ushort[]>();

        public static unsafe ushort[] ReadStaticArt(ushort graphic, out short width, out short height)
        {
            graphic &= FileManager.GraphicMask;

            (int length, int extra, bool patcher) = _file.SeekByEntryIndex(graphic + 0x4000);

            _file.Skip(4);

            width = _file.ReadShort();
            height = _file.ReadShort();

            if (width <= 0 || height <= 0)
                return null;

            /* ushort[] pixels ;

             if (_pixels.TryGetValue(graphic, out pixels))
                 return pixels;*/

            ushort[] pixels = new ushort[width * height];

            ushort* ptr = (ushort*)_file.PositionAddress;
            //long datastart = _file.Position;

            //ushort[] lineoffsets = _file.ReadArray<ushort>(height*2);
            ushort* lineoffsets = ptr;
            byte* datastart = (byte*)(ptr) + (height * 2);
            /*+ height * 2*/

            //datastart += (height * 2);

            int x = 0;
            int y = 0;
            ushort xoffs = 0;
            ushort run = 0;

            ptr = (ushort*)(datastart + (lineoffsets[0] * 2));
           // _file.Seek(datastart + (lineoffsets[0] * 2));

            while (y < height)
            {
                 xoffs = *ptr++;
                 //ptr++;
                 run = *ptr++;
                 //ptr++;

                //xoffs = _file.ReadUShort();
               // run = _file.ReadUShort();

                if (xoffs + run >= 2048)
                {
                    pixels = new ushort[width * height];
                    return pixels;
                }
                else if (xoffs + run != 0)
                {
                    x += xoffs;
                    int pos = y * width + x;
                    for (int j = 0; j < run; j++)
                    {
                        ushort val = *ptr++;
                       // ushort val = _file.ReadUShort();
                        if (val > 0)
                        {
                            val = (ushort)(0x8000 | val);
                        }
                        pixels[pos++] = val;
                    }
                    x += run;
                }
                else
                {
                    x = 0;
                    y++;
                    //_file.Seek(datastart + (lineoffsets[y] * 2));
                    ptr = (ushort*)(datastart + (lineoffsets[y] * 2));
                }
            }

            if ((graphic >= 0x2053 && graphic <= 0x2062)
                || (graphic >= 0x206A && graphic <= 0x2079))
            {
                for (int i = 0; i < width; i++)
                {
                    pixels[i] = 0;
                    pixels[(height - 1) * width + i] = 0;
                }

                for (int i = 0; i < height; i++)
                {
                    pixels[i * width] = 0;
                    pixels[i * width + width - 1] = 0;
                }
            }

          //  _pixels[graphic] = pixels;

            return pixels;
        }

        private static ushort[] _landArray = new ushort[44 * 44];

        public static ushort[] ReadLandArt(ushort graphic)
        {
            graphic &= FileManager.GraphicMask;

            (int length, int extra, bool patcher) = _file.SeekByEntryIndex(graphic);

            //ushort[] pixels = new ushort[44 * 44];

            Array.Clear(_landArray, 0, _landArray.Length);

            for (int i = 0; i < 22; i++)
            {
                int start = (22 - (i + 1));
                int pos = i * 44 + start;
                int end = start + (i + 1) * 2;

                for (int j = start; j < end; j++)
                {
                    ushort val = _file.ReadUShort();
                    if (val > 0)
                        val = (ushort)(0x8000 | val);

                    _landArray[pos++] = val;
                }
            }
            for (int i = 0; i < 22; i++)
            {
                int pos = (i + 22) * 44 + i;
                int end = i + (22 - i) * 2;

                for (int j = i; j < end; j++)
                {
                    ushort val = _file.ReadUShort();
                    if (val > 0)
                        val = (ushort)(0x8000 | val);

                    _landArray[pos++] = val;
                }
            }

            return _landArray;
        }
    }
}