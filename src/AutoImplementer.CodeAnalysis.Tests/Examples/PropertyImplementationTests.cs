﻿/*
   Copyright 2024 Alexander Stärk

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

namespace Basilisque.AutoImplementer.CodeAnalysis.Tests;


/* Define the interfaces that should be automatically implemented */

[AutoImplementInterface()]
public interface ITitle
{
    string Title { get; set; }
}

[AutoImplementInterface()]
public interface IDetails
{
    byte[]? Image { get; set; }
    string Summary { get; set; }
}


/* Define some example classes that implement the interfaces */

public partial class Book : ITitle, IDetails
{ /* will have the properties Title, Image and Summary */ }

public partial class Movie : ITitle, IDetails
{ /* will have the properties Title, Image and Summary */ }

public partial class Song : ITitle
{ /* will have the property Title */ }



/*
    Write some code to demonstrate that the classes work like expected.
    The tests themselves obviously don't provide much value. When the project compiles successfully, the source generator did it's work.
*/

[TestClass]
public partial class PropertyImplementationTests
{
    [TestMethod]
    public void Ensure_Properties_of_Book_are_working_fine()
    {
        const string TITLE = "Title";
        const string SUMMARY = "This is the summary";

        var book = new Book()
        {
            Title = TITLE,
            Summary = SUMMARY,
            Image = null
        };

        Assert.AreEqual(TITLE, book.Title);
        Assert.AreEqual(SUMMARY, book.Summary);
        Assert.IsNull(book.Image);
    }

    [TestMethod]
    public void Ensure_Properties_of_Movie_are_working_fine()
    {
        const string TITLE = "Title";
        const string SUMMARY = "This is the summary";

        var book = new Movie()
        {
            Title = TITLE,
            Summary = SUMMARY,
            Image = null
        };

        Assert.AreEqual(TITLE, book.Title);
        Assert.AreEqual(SUMMARY, book.Summary);
        Assert.IsNull(book.Image);
    }

    [TestMethod]
    public void Ensure_Properties_of_Song_are_working_fine()
    {
        const string TITLE = "Title";

        var book = new Song()
        {
            Title = TITLE
        };

        Assert.AreEqual(TITLE, book.Title);
    }
}