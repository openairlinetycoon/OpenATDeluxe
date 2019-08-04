//
// SCSharp.Mpq.Smk
//
// Authors:
//	Chris Toshok (toshok@gmail.com)
//
// Copyright 2006-2010 Chris Toshok
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Collections.Generic;
using System.Text;
//using NUnit.Framework;
using System.IO;

public class Huffmantree {
	protected internal class Node {
		//"0"- branch
		private Node left;

		public Node Left {
			get { return left; }
			set { left = value; }
		}
		//"1"-branch
		private Node right;

		public Node Right {
			get { return right; }
			set { right = value; }
		}
		//Made public for more speed
		public bool isLeaf;

		public bool IsLeaf {
			get { return isLeaf; }
			set { isLeaf = value; }
		}
		private int value;

		public int Value {
			get { return this.value; }
			set { this.value = value; }
		}

		public void Print(string prefix) {

			System.Console.WriteLine(prefix + ((IsLeaf) ? "Leaf: " + Value.ToString() : "No Leaf"));
			if (left != null) left.Print(prefix + " L:");
			if (right != null) right.Print(prefix + " R:");
		}
		public void Print() {
			Print("");
		}
	}


	/// <summary>
	/// Builds a new 8-bit huffmantree. The used algorithm is based on 
	/// http://wiki.multimedia.cx/index.php?title=Smacker#Packed_Huffman_Trees
	/// </summary>
	/// <param name="m">The stream to build the tree from</param>
	public virtual void BuildTree(BitStream m) {

		//Read tag
		int tag = m.ReadBits(1);
		//If tag is zero, finish
		if (tag == 0) return;

		//Init tree
		rootNode = new Node();
		BuildTree(m, rootNode);
		//For some reason we have to skip a bit
		m.ReadBits(1);

	}

	/// <summary>
	/// Decodes a value using this tree based on the next bits in the specified stream
	/// </summary>
	/// <param name="m">The stream to read bits from</param>
	public virtual int Decode(BitStream m) {
		Node currentNode = RootNode;
		if (currentNode == null)
			return 0;
		while (!currentNode.isLeaf) {
			int bit = m.ReadBits(1);
			if (bit == 0) {
				currentNode = currentNode.Left;
			} else {
				currentNode = currentNode.Right;
			}
		}
		return currentNode.Value;
	}

	protected virtual void BuildTree(BitStream m, Node current) {
		//Read flag
		int flag = m.ReadBits(1);
		//If flag is nonzero
		if (flag != 0) {
			//Advance to "0"-branch
			Node left = new Node();
			//Recursive call
			BuildTree(m, left);

			//The first left-node is actually the root
			if (current == null) {
				rootNode = left;
				return;
			} else
				current.Left = left;
		} else //If flag is zero
		  {
			if (current == null) {
				current = new Node();
				rootNode = current;

			}
			//Read 8 bit leaf
			int leaf = m.ReadBits(8);
			//Console.WriteLine("Decoded :" + leaf);
			current.IsLeaf = true;
			current.Value = leaf;
			return;
		}

		//Continue on the "1"-branch
		current.Right = new Node();
		BuildTree(m, current.Right);
	}
	Node rootNode;

	internal Node RootNode {
		get { return rootNode; }
		set { rootNode = value; }
	}
	public void PrintTree() {
		rootNode.Print();
	}


}

public class BigHuffmanTree : Huffmantree {

	/// <summary>
	/// Decodes a value using this tree based on the next bits in the specified stream
	/// </summary>
	/// <param name="m">The stream to read bits from</param>
	public override int Decode(BitStream m) {
		//int v = base.Decode(m);
		Node currentNode = RootNode;
		if (currentNode == null)
			return 0;
		while (!currentNode.isLeaf) {
			int bit = m.ReadBits(1);
			if (bit == 0) {
				currentNode = currentNode.Left;
			} else {
				currentNode = currentNode.Right;
			}
		}

		int v = currentNode.Value;

		if (v != iMarker1) {
			iMarker3 = iMarker2;
			iMarker2 = iMarker1;
			iMarker1 = v;

			marker3.Value = marker2.Value;
			marker2.Value = marker1.Value;
			marker1.Value = v;

		}
		return v;
	}
	/// <summary>
	/// Resets the dynamic decoder markers to zero
	/// </summary>
	public void ResetDecoder() {
		marker1.Value = 0;
		marker2.Value = 0;
		marker3.Value = 0;

		iMarker1 = 0;
		iMarker2 = 0;
		iMarker3 = 0;
	}

	Huffmantree highByteTree;
	Huffmantree lowByteTree;

	Node marker1;
	Node marker2;
	Node marker3;

	int iMarker1, iMarker2, iMarker3;
	public override void BuildTree(BitStream m) {
		//Read tag
		int tag = m.ReadBits(1);
		//If tag is zero, finish
		if (tag == 0) return;
		lowByteTree = new Huffmantree();
		lowByteTree.BuildTree(m);


		highByteTree = new Huffmantree();
		highByteTree.BuildTree(m);


		iMarker1 = m.ReadBits(16);
		//System.Console.WriteLine("M1:" + iMarker1);
		iMarker2 = m.ReadBits(16);
		//System.Console.WriteLine("M2:" + iMarker2);
		iMarker3 = m.ReadBits(16);
		//System.Console.WriteLine("M3:" + iMarker3);
		RootNode = new Node();
		BuildTree(m, RootNode);

		//For some reason we have to skip a bit
		m.ReadBits(1);

		if (marker1 == null) {
			// System.Console.WriteLine("Not using marker 1");
			marker1 = new Node();
		}
		if (marker2 == null) {
			//  System.Console.WriteLine("Not using marker 2");
			marker2 = new Node();
		}
		if (marker3 == null) {
			//   System.Console.WriteLine("Not using marker 3");
			marker3 = new Node();
		}

	}
	protected override void BuildTree(BitStream m, Node current) {
		//Read flag
		int flag = m.ReadBits(1);
		//If flag is nonzero
		if (flag != 0) {
			//Advance to "0"-branch
			Node left = new Node();
			//Recursive call
			BuildTree(m, left);

			//The first left-node is actually the root
			if (current == null) {
				RootNode = left;
				return;
			} else
				current.Left = left;
		} else //If flag is zero
		  {
			if (current == null) {
				current = new Node();
				RootNode = current;
			}
			//Read 16 bit leaf by decoding the low byte, then the high byte
			int lower = lowByteTree.Decode(m);
			int higher = highByteTree.Decode(m);
			int leaf = lower | (higher << 8);
			//System.Console.WriteLine("Decoded: " + leaf);
			//If we found one of the markers, store pointers to those nodes.
			if (leaf == iMarker1) {
				leaf = 0;
				marker1 = current;
			}
			if (leaf == iMarker2) {
				leaf = 0;
				marker2 = current;
			}
			if (leaf == iMarker3) {
				leaf = 0;
				marker3 = current;
			}

			current.IsLeaf = true;
			current.Value = leaf;
			return;
		}

		//Continue on the "1"-branch
		current.Right = new Node();
		BuildTree(m, current.Right);
	}
}

//This should move to another file
// [TestFixture]
// public class HuffmantreeTest
// {

// [Test]
// public void TestBuild()
// {
//Set up a memory stream first
// MemoryStream stream = new MemoryStream();
// stream.WriteByte(0x6F); //These bytes correspond to the example 
// stream.WriteByte(0x20); //bit-sequence on 
// stream.WriteByte(0x02); //http://wiki.multimedia.cx/index.php?title=Smacker#Packed_Huffman_Trees
// stream.WriteByte(0x05);
// stream.WriteByte(0x0C);
// stream.WriteByte(0x3A);
// stream.WriteByte(0x80);
// stream.WriteByte(0x00);            
// BitStream bs = new BitStream(stream);
// bs.Reset();

//Build a new tree
// Huffmantree tree = new Huffmantree();
// tree.TestDecodeTree(bs);

//Our tree should look like this:
// Assert.AreEqual(tree.RootNode.Left.Left.Left.Value, 3);
// Assert.AreEqual(tree.RootNode.Left.Left.Right.Left.Value, 4);
// Assert.AreEqual(tree.RootNode.Left.Left.Right.Right.Value, 5);
// Assert.AreEqual(tree.RootNode.Left.Right.Value, 6);
// Assert.AreEqual(tree.RootNode.Right.Left.Value, 7);
// Assert.AreEqual(tree.RootNode.Right.Right.Value, 8);
// }

// [Test]
// public void TestDecode()
// {
//Set up a memory stream first
// MemoryStream stream = new MemoryStream();
// stream.WriteByte(0x6F); //These bytes correspond to the example 
// stream.WriteByte(0x20); //bit-sequence on 
// stream.WriteByte(0x02); //http://wiki.multimedia.cx/index.php?title=Smacker#Packed_Huffman_Trees
// stream.WriteByte(0x05);
// stream.WriteByte(0x0C);
// stream.WriteByte(0x3A);
// stream.WriteByte(0x80);
// stream.WriteByte(0x00);
// BitStream bs = new BitStream(stream);
// bs.Reset();

//Build a new tree
// Huffmantree tree = new Huffmantree();
// tree.TestDecodeTree(bs);

// stream = new MemoryStream();
// stream.WriteByte(0x20); //Bit stream that visits every leaf
// stream.WriteByte(0xB6); //000 ->3 0010 ->4 0011->5 01 -> 6 10 -> 7 11 -> 8
// stream.WriteByte(0x01);
// bs = new BitStream(stream);
// bs.Reset();
// Assert.AreEqual(tree.Decode(bs), 3);
// Assert.AreEqual(tree.Decode(bs), 4);
// Assert.AreEqual(tree.Decode(bs), 5);
// Assert.AreEqual(tree.Decode(bs), 6);
// Assert.AreEqual(tree.Decode(bs), 7);
// Assert.AreEqual(tree.Decode(bs), 8);
// }
// }
