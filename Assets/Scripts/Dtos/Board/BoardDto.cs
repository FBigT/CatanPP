using Assets.Scripts.Enums;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Dtos.Board
{
    // BoardStateDto.cs
    public class BoardStateDto
    {
        public List<HexTileDto> hexTiles;
        public ThiefDto thief;
        public List<VertexDto> vertices;
        public List<EdgeDto> edges;
        public List<PortDto> ports;
    }

    // HexTileDto.cs
    public class HexTileDto
    {
        public int q;
        public int r;
        public string resourceType;
        public int numberToken;
    }

    // ThiefDto.cs
    public class ThiefDto
    {
        public int q;
        public int r;
    }

    // VertexDto.cs
    public class VertexDto
    {
        public float x;
        public float z;
        public StructureType? structure; // null if none
        public string owner; // null if none
    }

    // EdgeDto.cs
    public class EdgeDto
    {
        public float ax, az;
        public float bx, bz;
        public string owner; // null if none
    }

    // PortDto.cs
    public class PortDto
    {
        public float ax, az;
        public float bx, bz;
        public PortType portType;
    }
}
