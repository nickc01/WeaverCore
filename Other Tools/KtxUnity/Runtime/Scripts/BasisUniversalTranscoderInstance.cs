// Copyright (c) 2019-2022 Andreas Atteneder, All Rights Reserved.

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


using System.Runtime.InteropServices;
using System;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Profiling;


namespace KtxUnity {

    // Source: basisu_transcoder.h -> basist::basis_texture_type
    public enum BasisUniversalTextureType {
        /// <summary>
        /// An arbitrary array of 2D RGB or RGBA images with optional mipmaps,
        /// array size = # images, each image may have a different resolution
        /// and # of mipmap levels
        /// </summary>
        Image2D = 0,
        
        /// <summary>
        /// An array of 2D RGB or RGBA images with optional mipmaps,
        /// array size = # images, each image has the same resolution and
        /// mipmap levels
        /// </summary>
        Image2DArray = 1,
        
        /// <summary>
        /// An array of cubemap levels,
        /// total # of images must be divisable by 6,
        /// in X+, X-, Y+, Y-, Z+, Z- order, with optional mipmaps
        /// </summary>
        CubemapArray = 2,
        
        /// <summary>
        /// An array of 2D video frames, with optional mipmaps,
        /// # frames = # images, each image has the same resolution and # of
        /// mipmap levels
        /// </summary>
        VideoFrames = 3,
        
        /// <summary>
        /// A 3D texture with optional mipmaps, Z dimension = # images,
        /// each image has the same resolution and # of mipmap levels
        /// </summary>
        Volume = 4,
    }

    public class BasisUniversalTranscoderInstance {
        public IntPtr nativeReference;

        public BasisUniversalTranscoderInstance( IntPtr nativeReference ) {
            this.nativeReference = nativeReference;
        }

        public unsafe bool Open(NativeSlice<byte> data) {
            void* src = data.GetUnsafeReadOnlyPtr();
            bool success = ktx_basisu_open_basis(nativeReference,src,data.Length);
#if DEBUG
            if(!success) {
                Debug.LogError("Couldn't validate BasisU header!");
            }
#endif
            return success;
        }
        
        public MetaData LoadMetaData() {
            Profiler.BeginSample("LoadMetaData");
            MetaData meta = new MetaData();
            meta.hasAlpha = GetHasAlpha();
            var imageCount = GetImageCount();
            meta.images = new ImageInfo[imageCount];
            for(uint i=0; i<imageCount;i++) {
                var ii = new ImageInfo();
                var levelCount = GetLevelCount(i);
                ii.levels = new LevelInfo[levelCount];
                for (uint l = 0; l < levelCount; l++)
                {
                    var li = new LevelInfo();
                    GetImageSize(out li.width, out li.height,i,l);
                    ii.levels[l] = li;
                }
                meta.images[i] = ii;
            }
            Profiler.EndSample();
            return meta;
        }

        public void Close() {
            ktx_basisu_close_basis(nativeReference);
        }

        public bool GetHasAlpha() {
            return ktx_basisu_getHasAlpha(nativeReference);
        }

        public uint GetImageCount() {
            return ktx_basisu_getNumImages(nativeReference);
        }

        public uint GetLevelCount(uint imageIndex) {
            return ktx_basisu_getNumLevels(nativeReference,imageIndex);
        }

        public void GetImageSize( out uint width, out uint height, System.UInt32 image_index = 0, System.UInt32 level_index = 0) {
            width = ktx_basisu_getImageWidth(nativeReference,image_index,level_index);
            height = ktx_basisu_getImageHeight(nativeReference,image_index,level_index);
        }

        public bool GetYFlip() {
            return !ktx_basisu_get_y_flip(nativeReference);
        }

        // public bool GetIsEtc1s() {
        //     return ktx_basisu_get_is_etc1s(nativeReference);
        // }

        public BasisUniversalTextureType GetTextureType() {
            return ktx_basisu_get_texture_type(nativeReference);
        }

        public uint GetImageTranscodedSize(uint imageIndex, uint levelIndex, TranscodeFormat format) {
            return ktx_basisu_getImageTranscodedSizeInBytes(nativeReference,imageIndex,levelIndex,(uint)format);
        }

        public unsafe bool Transcode(uint imageIndex,uint levelIndex,TranscodeFormat format,out byte[] transcodedData ) {
            Profiler.BeginSample("BasisU.Transcode");
            transcodedData = null;

            if(!ktx_basisu_startTranscoding(nativeReference)) {
                Profiler.EndSample();
                return false;
            }

            var size = GetImageTranscodedSize(imageIndex,levelIndex,format);
            byte[] data = new byte[size];

            bool result = false;
            fixed( void* dst = &(data[0]) ) {
                result = ktx_basisu_transcodeImage(nativeReference,dst,size,imageIndex,levelIndex,(uint)format,0,0);
            }
            transcodedData = data;
            Profiler.EndSample();
            return result;
        }

        ~BasisUniversalTranscoderInstance() {
            ktx_basisu_delete_basis(nativeReference);
        }

        unsafe delegate bool ktx_basisu_open_basisDelegate( IntPtr basis, void * data, int length );
        static ktx_basisu_open_basisDelegate ktx_basisu_open_basisInternal;
        static ktx_basisu_open_basisDelegate ktx_basisu_open_basis => ktx_basisu_open_basisInternal ??= KTXNativeManager.GetProcInKTXUnity<ktx_basisu_open_basisDelegate>(nameof(ktx_basisu_open_basis));

        delegate void ktx_basisu_close_basisDelegate( IntPtr basis );
        static ktx_basisu_close_basisDelegate ktx_basisu_close_basisInternal;
        static ktx_basisu_close_basisDelegate ktx_basisu_close_basis => ktx_basisu_close_basisInternal ??= KTXNativeManager.GetProcInKTXUnity<ktx_basisu_close_basisDelegate>(nameof(ktx_basisu_close_basis));

        delegate void ktx_basisu_delete_basisDelegate( IntPtr basis );
        static ktx_basisu_delete_basisDelegate ktx_basisu_delete_basisInternal;
        static ktx_basisu_delete_basisDelegate ktx_basisu_delete_basis => ktx_basisu_delete_basisInternal ??= KTXNativeManager.GetProcInKTXUnity<ktx_basisu_delete_basisDelegate>(nameof(ktx_basisu_delete_basis));

        delegate bool ktx_basisu_getHasAlphaDelegate( IntPtr basis );
        static ktx_basisu_getHasAlphaDelegate ktx_basisu_getHasAlphaInternal;
        static ktx_basisu_getHasAlphaDelegate ktx_basisu_getHasAlpha => ktx_basisu_getHasAlphaInternal ??= KTXNativeManager.GetProcInKTXUnity<ktx_basisu_getHasAlphaDelegate>(nameof(ktx_basisu_getHasAlpha));

        delegate System.UInt32 ktx_basisu_getNumImagesDelegate( IntPtr basis );
        static ktx_basisu_getNumImagesDelegate ktx_basisu_getNumImagesInternal;
        static ktx_basisu_getNumImagesDelegate ktx_basisu_getNumImages => ktx_basisu_getNumImagesInternal ??= KTXNativeManager.GetProcInKTXUnity<ktx_basisu_getNumImagesDelegate>(nameof(ktx_basisu_getNumImages));

        delegate System.UInt32 ktx_basisu_getNumLevelsDelegate( IntPtr basis, System.UInt32 image_index);
        static ktx_basisu_getNumLevelsDelegate ktx_basisu_getNumLevelsInternal;
        static ktx_basisu_getNumLevelsDelegate ktx_basisu_getNumLevels => ktx_basisu_getNumLevelsInternal ??= KTXNativeManager.GetProcInKTXUnity<ktx_basisu_getNumLevelsDelegate>(nameof(ktx_basisu_getNumLevels));

        delegate System.UInt32 ktx_basisu_getImageWidthDelegate( IntPtr basis, System.UInt32 image_index, System.UInt32 level_index);
        static ktx_basisu_getImageWidthDelegate ktx_basisu_getImageWidthInternal;
        static ktx_basisu_getImageWidthDelegate ktx_basisu_getImageWidth => ktx_basisu_getImageWidthInternal ??= KTXNativeManager.GetProcInKTXUnity<ktx_basisu_getImageWidthDelegate>(nameof(ktx_basisu_getImageWidth));

        delegate System.UInt32 ktx_basisu_getImageHeightDelegate( IntPtr basis, System.UInt32 image_index, System.UInt32 level_index);
        static ktx_basisu_getImageHeightDelegate ktx_basisu_getImageHeightInternal;
        static ktx_basisu_getImageHeightDelegate ktx_basisu_getImageHeight => ktx_basisu_getImageHeightInternal ??= KTXNativeManager.GetProcInKTXUnity<ktx_basisu_getImageHeightDelegate>(nameof(ktx_basisu_getImageHeight));

        delegate bool ktx_basisu_get_y_flipDelegate( IntPtr basis );
        static ktx_basisu_get_y_flipDelegate ktx_basisu_get_y_flipInternal;
        static ktx_basisu_get_y_flipDelegate ktx_basisu_get_y_flip => ktx_basisu_get_y_flipInternal ??= KTXNativeManager.GetProcInKTXUnity<ktx_basisu_get_y_flipDelegate>(nameof(ktx_basisu_get_y_flip));
        
        // [DllImport(KtxNativeInstance.INTERFACE_DLL)]
        // private static extern bool ktx_basisu_get_is_etc1s( IntPtr basis );
        
        delegate BasisUniversalTextureType ktx_basisu_get_texture_typeDelegate( IntPtr basis );
        static ktx_basisu_get_texture_typeDelegate ktx_basisu_get_texture_typeInternal;
        static ktx_basisu_get_texture_typeDelegate ktx_basisu_get_texture_type => ktx_basisu_get_texture_typeInternal ??= KTXNativeManager.GetProcInKTXUnity<ktx_basisu_get_texture_typeDelegate>(nameof(ktx_basisu_get_texture_type));

        delegate System.UInt32 ktx_basisu_getImageTranscodedSizeInBytesDelegate( IntPtr basis, System.UInt32 image_index, System.UInt32 level_index, System.UInt32 format);
        static ktx_basisu_getImageTranscodedSizeInBytesDelegate ktx_basisu_getImageTranscodedSizeInBytesInternal;
        static ktx_basisu_getImageTranscodedSizeInBytesDelegate ktx_basisu_getImageTranscodedSizeInBytes => ktx_basisu_getImageTranscodedSizeInBytesInternal ??= KTXNativeManager.GetProcInKTXUnity<ktx_basisu_getImageTranscodedSizeInBytesDelegate>(nameof(ktx_basisu_getImageTranscodedSizeInBytes));

        delegate bool ktx_basisu_startTranscodingDelegate( IntPtr basis );
        static ktx_basisu_startTranscodingDelegate ktx_basisu_startTranscodingInternal;
        static ktx_basisu_startTranscodingDelegate ktx_basisu_startTranscoding => ktx_basisu_startTranscodingInternal ??= KTXNativeManager.GetProcInKTXUnity<ktx_basisu_startTranscodingDelegate>(nameof(ktx_basisu_startTranscoding));

        unsafe delegate bool ktx_basisu_transcodeImageDelegate( IntPtr basis, void * dst, uint dst_size, System.UInt32 image_index, System.UInt32 level_index, System.UInt32 format, System.UInt32 pvrtc_wrap_addressing, System.UInt32 get_alpha_for_opaque_formats);
        static ktx_basisu_transcodeImageDelegate ktx_basisu_transcodeImageInternal;
        static ktx_basisu_transcodeImageDelegate ktx_basisu_transcodeImage => ktx_basisu_transcodeImageInternal ??= KTXNativeManager.GetProcInKTXUnity<ktx_basisu_transcodeImageDelegate>(nameof(ktx_basisu_transcodeImage));
    }
}