using System.Collections.Generic;
using NUnit.Framework;

namespace Unity.FilmInternalUtilities.Tests {

//[TODO-sin: 2022-1-6] Move to FilmInternalUtilities
internal class ListExtensionsTest {

    [Test]
    public void MoveListElements() {

        List<int> l = new List<int>() { 1, 2, 3, 4, 5 };

        l.Move(2, 0);       
        Assert.IsTrue(l.AreElementsEqual(new List<int>() {3, 1, 2, 4, 5}));
        
        l.Move(3, 3);
        Assert.IsTrue(l.AreElementsEqual(new List<int>() {3, 1, 2, 4, 5}));

        l.Move(4, 3);
        Assert.IsTrue(l.AreElementsEqual(new List<int>() {3, 1, 2, 5, 4}));
        
        l.Move(0, 4);
        Assert.IsTrue(l.AreElementsEqual(new List<int>() {1, 2, 5, 4, 3}));

        l.Move(1, 3);
        Assert.IsTrue(l.AreElementsEqual(new List<int>() {1, 5, 4, 2, 3}));

        l.Move(0, 2);        
        Assert.IsTrue(l.AreElementsEqual(new List<int>() {5, 4, 1, 2, 3}));        
    }
    
}
} //end namespace
