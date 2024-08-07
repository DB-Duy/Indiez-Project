This scene exemplifies using material variations with the Crowd Animator Workflow. 
The demo script (FishSwarmBoid.cs) defines a Vector4 variation buffer for the texture uvs and offset
to be used with the texture atlases and in the shader the respective texture position is used with this uv and offset. 

This way, all the instances of the fish in this scene are rendered with a single draw call. 
The instances are also further manipulated with a compute shader (GPUIFishBoids.compute) to implement boids behavior.