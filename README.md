# AR Foundation Depth Map

Sample project to visualize the depth map generated by the platform's Augmented Reality system – e.g., Google ARCore.

![](https://raw.githubusercontent.com/andijakl/arfoundation-depth/main/AR-Foundation-Unity-Depth-Maps-Project.gif)

## What is this about?

Start the app and slightly move the phone. The depth map is shown in directly in the middle of the live camera stream as a picture-in-picture.

## How can I learn more?

The tutorial and background info is available in the blog post series [Easily Create a Depth Map with Smartphone AR](https://www.andreasjakl.com/easily-create-depth-maps-with-smartphone-ar-part-1/).

### Details

The depth map is acquired in 32-bit floating-point format. The example directly shows this to an RGB texture. As such, the depth map is visualized in red and clipped to the near area (given the max 256 red values). You could apply a [shader](https://github.com/Unity-Technologies/arfoundation-samples/blob/6296272a416925b56ce85470e0c7bef5c913ec0c/Assets/Shaders/DepthGradient.shader) to convert the depth map to a color-coded depth texture preserving the full possible depth range. However, for a quick visualization of the nearby depth, the direct display works well!

## Credits

This project is a simpler version of the [Unity AR Foundation sample](https://github.com/Unity-Technologies/arfoundation-samples). It focuses specifically on the depth map generation, leaving all other aspects out of the scene. You can also read a short intro in the [Google ARCore Depth API Developer Guide for Unity](https://developers.google.com/ar/develop/unity/depth/developer-guide).

Released under the MIT License - see the LICENSE file for details.

Developed by Andreas Jakl, Professor at the St. Pölten University of Applied Sciences, Austria.

* <https://www.fhstp.ac.at/>
* <https://www.andreasjakl.com/>
* <https://twitter.com/andijakl>