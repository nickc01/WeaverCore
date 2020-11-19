using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;


/* NOTE: THIS CODE IS PRETTY BAD. I SHOULD HOPEFULLY HAVE THIS ALL CLEANED UP SOON.
 * I TRIED TO CLEAN IT UP SOME TIME AGO, BUT IT WOULD KEEP BREAKING THIS CODE
 * EVENTUALLY I WILL GET TO IT AGAIN
 * 
 * 
 */


namespace WeaverCore.Utilities
{
    public static class TextureUtilities
	{
        static Vector2 RotateVector(Vector2 vector, RotationType rotation)
        {
            return RotateVectorAroundPoint(vector, rotation, default(Vector2));
        }

        static Vector2 RotateVectorAroundPoint(Vector2 vector, RotationType rotation, Vector2 pointOfRotation)
        {
            Matrix4x4 rotationMatrix = Matrix4x4.Translate(new Vector3(pointOfRotation.x, pointOfRotation.y));
            rotationMatrix *= Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, -(int)rotation));
            rotationMatrix *= Matrix4x4.Translate(new Vector3(-pointOfRotation.x, -pointOfRotation.y));

            return rotationMatrix.MultiplyPoint3x4(vector);
        }


        public static void Rotate(this Texture2D texture, RotationType rotation)
        {
            var destWidth = texture.width;
            var destHeight = texture.height;

            if (rotation == RotationType.Left || rotation == RotationType.Right)
            {
                destWidth = texture.height;
                destHeight = texture.width;
            }

            var pixels = texture.GetPixels();
            var newPixels = new Color[pixels.GetLength(0)];

            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    

                    var newPoint = RotateVectorAroundPoint(new Vector2(x, y), rotation, new Vector2((texture.width - 1) / 2f,(texture.height - 1) / 2f));

                    newPoint.x -= ((texture.width - 1) / 2f);
                    newPoint.y -= ((texture.height - 1) / 2f);

                    newPoint.x += ((destWidth - 1) / 2f);
                    newPoint.y += ((destHeight - 1) / 2f);



                    int newX = Mathf.RoundToInt(newPoint.x);
                    int newY = Mathf.RoundToInt(newPoint.y);

                    newPixels[newX + (destWidth * newY)] = pixels[x + (texture.width * y)];
                    /*try
                    {
                       
                    }
                    catch (Exception e)
                    {
                        var origin = new Vector2((texture.width) / 2f, (texture.height) / 2f);
                        Debug.Log("Rotation Type = " + rotation);
                        Debug.Log("Old Size = " + new Vector2(texture.width,texture.height));
                        Debug.Log("New Size = " + new Vector2(destWidth, destHeight));
                        Debug.Log("Old Point = " + new Vector2(x, y));
                        Debug.Log("Old Point Relative = " + new Vector2(x - origin.x, y - origin.y));
                        Debug.Log("New Point = " + newPoint);
                        Debug.Log("New Point Relative = " + new Vector2(newPoint.x - origin.x, newPoint.y - origin.y));
                        throw;
                    }*/
                }
            }

            //newTex.Apply();

            //return newTex;

            texture.Resize(destWidth, destHeight);

            texture.SetPixels(newPixels);

            texture.Apply();
        }



        /*public static Texture2D Rotate(this Texture2D texture, RotationType rotation)
        {
            Vector2 originalDimensions = new Vector2Int(texture.width, texture.height);
            Vector2 dimensions = RotateVector(originalDimensions, rotation);
            //Vector3 dimensions = new Vector3(matrix.GetLength(0), matrix.GetLength(1));

            //Debug.Log("Old Dimensions = " + dimensions);
            //Debug.Log("Rotation Type = " + rotation);
            //dimensions = sizeRotationMatrix.MultiplyPoint3x4(dimensions);
            Debug.LogError("Original Dimensions = " + originalDimensions);
            Debug.LogError("New Dimensions = " + dimensions);

            Texture2D newTexture = new Texture2D(Mathf.Abs(Mathf.RoundToInt(dimensions.x)), Mathf.Abs(Mathf.RoundToInt(dimensions.y)));

            //T[,] newMatrix = new T[Mathf.Abs(Mathf.RoundToInt(dimensions.x)), Mathf.Abs(Mathf.RoundToInt(dimensions.y))];

            for (int x = 0; x < originalDimensions.x; x++)
            {
                for (int y = 0; y < originalDimensions.y; y++)
                {
                    Debug.LogError("Point = " + new Vector2(x,y));
                    Vector2 point = RotateVectorAroundPoint(new Vector2(x, y), rotation, new Vector2((originalDimensions.x - 1) / 2f, (originalDimensions.y - 1) / 2f));
                }
            }
            newTexture.Apply();

            return newTexture;
        }*/


        /*/// <summary>
        /// Takes in a matrix, makes a copy, and rotates the copy based on the rotation parameter
        /// </summary>
        /// <typeparam name="T">The type of each element in the matrix</typeparam>
        /// <param name="matrix">The matrix to rotate</param>
        /// <param name="rotation">How much the matrix should be rotated</param>
        /// <returns>The new, rotated matrix</returns>
        public static T[,] Rotate<T>(T[,] matrix, RotationType rotation)
        {
            Matrix4x4 centerRotation = Matrix4x4.Translate(new Vector3((matrix.GetLength(0) - 1) / 2.0f, (matrix.GetLength(1) - 1) / 2.0f));
            centerRotation *= Matrix4x4.Rotate(Quaternion.Euler(0f,0f,-(int)rotation));
            centerRotation *= Matrix4x4.Translate(new Vector3((matrix.GetLength(0) - 1) / -2.0f, (matrix.GetLength(1) - 1) / -2.0f));

            Matrix4x4 sizeRotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, -(int)rotation));

            Vector2Int originalDimensions = new Vector2Int(matrix.GetLength(0), matrix.GetLength(1));
            Vector3 dimensions = new Vector3(matrix.GetLength(0), matrix.GetLength(1));

            Debug.Log("Old Dimensions = " + dimensions);
            Debug.Log("Rotation Type = " + rotation);
            dimensions = sizeRotationMatrix.MultiplyPoint3x4(dimensions);
            Debug.Log("New Dimensions = " + dimensions);

            T[,] newMatrix = new T[Mathf.Abs(Mathf.RoundToInt(dimensions.x)), Mathf.Abs(Mathf.RoundToInt(dimensions.y))];

            for (int x = 0; x < originalDimensions.x; x++)
            {
                for (int y = 0; y < originalDimensions.y; y++)
                {
                    Vector3 point = new Vector3(x,y);
                    Debug.Log("Point = " + point);
                    var transPoint = centerRotation.MultiplyPoint3x4(point);
                    Debug.Log("Transformed Point = " + transPoint);
                    newMatrix[Mathf.RoundToInt(transPoint.x), Mathf.RoundToInt(transPoint.y)] = matrix[x,y];
                }
            }
            return matrix;
        }*/

        /*public static void FlipVertically(Texture2D texture)
        {
            FlipVertically((x, y) => texture.GetPixel(x,y), (x, y, c) => texture.SetPixel(x,y,c), texture.width, texture.height);
        }

        public static void FlipHorizontally(Texture2D texture)
        {
            FlipHorizontally((x, y) => texture.GetPixel(x, y), (x, y, c) => texture.SetPixel(x, y, c), texture.width, texture.height);
        }

        public static void FlipDiagonally(Texture2D texture)
        {
            FlipDiagonally((x, y) => texture.GetPixel(x, y), (x, y, c) => texture.SetPixel(x, y, c), texture.width, texture.height);
        }*/

        /*public static void FlipVertically<T>(T[,] matrix)
        {
            FlipVertically((x, y) => matrix[x, y], (x, y, c) => matrix[x, y] = c, matrix.GetLength(0), matrix.GetLength(1));
        }

        public static void FlipHorizontally<T>(T[,] matrix)
        {
            FlipHorizontally((x, y) => matrix[x, y], (x, y, c) => matrix[x, y] = c, matrix.GetLength(0), matrix.GetLength(1));
        }

        public static void FlipDiagonally<T>(T[,] matrix)
        {
            FlipDiagonally((x, y) => matrix[x, y], (x, y, c) => matrix[x, y] = c, matrix.GetLength(0), matrix.GetLength(1));
        }*/



        /* static void FlipVertically<T>(Func<int,int,T> getter, Action<int,int,T> setter, int width, int height)
         {
             T[,] matrix = new T[width,height];


             for (int x = 0; x < width; x++)
             {
                 for (int y = 0; y < height; y++)
                 {
                     matrix[x, y] = getter(x, height - y - 1);
                 }
             }

             for (int x = 0; x < width; x++)
             {
                 for (int y = 0; y < height; y++)
                 {
                     setter(x, y, matrix[x, y]);
                 }
             }
         }

         static void FlipHorizontally<T>(Func<int, int, T> getter, Action<int, int, T> setter, int width, int height)
         {
             T[,] matrix = new T[width, height];


             for (int x = 0; x < width; x++)
             {
                 for (int y = 0; y < height; y++)
                 {
                     matrix[x, y] = getter(width - 1 - x, y);
                 }
             }

             for (int x = 0; x < width; x++)
             {
                 for (int y = 0; y < height; y++)
                 {
                     setter(x, y, matrix[x, y]);
                 }
             }
         }*/
        public static void FlipVertically(Texture2D texture)
        {
            var width = texture.width;
            var height = texture.height;

            Color[,] newMatrix = new Color[width, height];


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    newMatrix[x, y] = texture.GetPixel(width - 1 - x, y);
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, newMatrix[x, y]);
                }
            }
        }

        public static void FlipHorizontally(Texture2D texture)
        {
            var width = texture.width;
            var height = texture.height;

            Color[,] newMatrix = new Color[width, height];


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    newMatrix[x, y] = texture.GetPixel(x, height - 1 - y);
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, newMatrix[x, y]);
                }
            }
        }

        public static void FlipDiagonally(Texture2D texture)
        {
            var width = texture.width;
            var height = texture.height;

            Color[,] newMatrix = new Color[width, height];


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    newMatrix[x, y] = texture.GetPixel(width - 1 - x, height - 1 - y);
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x,y,newMatrix[x, y]);
                }
            }
        }

        public static void FlipVertically<T>(T[,] matrix)
        {
            var width = matrix.GetLength(0);
            var height = matrix.GetLength(1);

            T[,] newMatrix = new T[width, height];


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    newMatrix[x, y] = matrix[width - 1 - x, y];
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    matrix[x, y] = newMatrix[x, y];
                }
            }
        }

        public static void FlipHorizontally<T>(T[,] matrix)
        {
            var width = matrix.GetLength(0);
            var height = matrix.GetLength(1);

            T[,] newMatrix = new T[width, height];


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    newMatrix[x, y] = matrix[x, height - 1 - y];
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    matrix[x, y] = newMatrix[x, y];
                }
            }
        }

        public static void FlipDiagonally<T>(T[,] matrix)
        {
            var width = matrix.GetLength(0);
            var height = matrix.GetLength(1);

            T[,] newMatrix = new T[width, height];


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    newMatrix[x, y] = matrix[width - 1 - x, height - 1 - y];
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    matrix[x, y] = newMatrix[x, y];
                }
            }
        }
    }
}
