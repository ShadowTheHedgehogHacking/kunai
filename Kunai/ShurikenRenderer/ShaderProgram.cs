using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Kunai.ShurikenRenderer
{
    public class ShaderProgram
    {
        public int Id { get; private set; } = 0;
        public string Name { get; private set; }

        /// <summary>
        /// Compiles the shader program using the specified vertex and fragment
        /// programs.
        /// </summary>
        /// <param name="vertexPath">The path to the vertex program.</param>
        /// <param name="fragmentPath">The path to the fragment program.</param>
        public void Compile(string in_Name, string in_VertexPath, string in_FragmentPath)
        {
            Name = in_Name;
            string vertexSource = "";
            string fragmentSource = "";

            using (StreamReader reader = new StreamReader(in_VertexPath))
            {
                vertexSource = reader.ReadToEnd();
            };

            using (StreamReader reader = new StreamReader(in_FragmentPath))
            {
                fragmentSource = reader.ReadToEnd();
            };

            // Create shaders
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSource);
            GL.CompileShader(vertexShader);

            string vLog = GL.GetShaderInfoLog(vertexShader);
            if (!string.IsNullOrEmpty(vLog))
                Console.WriteLine(vLog);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentSource);
            GL.CompileShader(fragmentShader);

            string fLog = GL.GetShaderInfoLog(fragmentShader);
            if (!string.IsNullOrEmpty(fLog))
                Console.WriteLine(fLog);

            // Link shaders to program
            Id = GL.CreateProgram();
            GL.AttachShader(Id, vertexShader);
            GL.AttachShader(Id, fragmentShader);
            GL.LinkProgram(Id);

            // Cleanup
            GL.DetachShader(Id, vertexShader);
            GL.DetachShader(Id, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        public void SetUniform(string in_Attribute, int in_Value)
        {
            GL.Uniform1(GL.GetUniformLocation(Id, in_Attribute), in_Value);
        }

        public void SetUniform(string in_Attribute, float in_Value)
        {
            GL.Uniform1(GL.GetUniformLocation(Id, in_Attribute), in_Value);
        }

        public void SetMatrix4(string in_Name, Matrix4 in_Mat)
        {
            GL.UniformMatrix4(GL.GetUniformLocation(Id, in_Name), true, ref in_Mat);
        }

        public void SetBool(string in_Name, bool in_Value)
        {
            GL.Uniform1(GL.GetUniformLocation(Id, in_Name), in_Value ? 1 : 0);
        }

        public void Use()
        {
            GL.UseProgram(Id);
        }

        public ShaderProgram(string in_Name, string in_VertexPath, string in_FragmentPath)
        {
            Compile(in_Name, in_VertexPath, in_FragmentPath);
        }

        public ShaderProgram()
        {

        }
    }
}
