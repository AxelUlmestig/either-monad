# How to handle errors with an Either monad
#### Introduction
Error handling is tricky and is traditionally done by throwing exceptions when working in an imperative language. There are however, other approaches that can be used to handle when things don't go as planned. One alternative is to use a data type that "contains" the errors. Here is an example of how to do that:

```csharp
class Either<T, U>
```

This class has two generic types associated with it that represents failure and success respectively. An instance of `Either<T, U>` can be in one of two states, it can either contain an error of type `T` or the successful result of some computation of type `U`.

The `Either` type has two static methods associated with it that can be used to create Either values.
```csharp
public static Either<T, U> Succeed(U value)
```
which represents a successful evaluation and
```csharp
public static Either<T, U> Fail(T err)
```
which represents an unsuccessful evaluation.

Now, how do we use this in practice? Imagine a method that takes a double value and inverts it. I.e. `x => 1 / x`. Now this has some pretty obvious potential for failure. We could write a method that checks for this and throws an exception like so:
```csharp
public static double Invert(double x)
{
    if(x == 0)
    {
        throw new Exception("Error: divide by zero");
    }
    return 1 / x;
}
```
If we were to use this we would need to wrap it in a try-catch block because we are not sure if it's going to return a double value even though the declaration says that it will.

Now, the same function can be written using the `Either` type to represent the potential failures instead:
```csharp
public static Either<string, double> Invert(double x)
{
    if(x == 0)
    {
        return Either<string, double>
            .Fail("Error: divide by zero");
    }
    return Either<string, double>
        .Succeed(1 / x);
}
```

Here we are much more explicit with what the possible results are. This function will always do what the signature says and return an `Either<string, double>`. Either it can succeed and return a double value or it can fail and return a string as an error message.

Now what happens when we try to use this method?

```csharp
Either<string, double> successOrFailure = Invert(4);
```
We need a way of extracting the values back out of the Either type so that we can do something useful with it. But at this point we don't know if the method succeeded or failed. How would such a method look?
```csharp
var result = successOrFailure.Extract();
```
But what will `var` be in this case? We don't know what the data type of the content is. What we need is a way for us to transform the contents to some type regardless of what the content is. To do that we need to specify how we handle the two possible states, failure or success.
```csharp
public V Extract<V>(Func<T, V> f, Func<U, V> g);
```
Here the first function, `f`, will transform the error state to some data type `V` and second function, `g`, will transform the success state to the same data type `V`. By supplying both of these functions we've specified what should happen regardless of what the internal state of the `Either` variable is in.

Now for the example we were using it would look like this:
```csharp
Either<string, double> successOrFailure = Invert(4);
string output = successOrFailure.Extract(
    errorMessage => errorMessage,
    inverted => "The inverse is: " + inverted.ToString()
);
```
Here the `output` variable will be a string explaining what the result of the operation was.

Now at this point we can do everything that can be done with the traditional throw, try and catch pattern.

But wait, there's more.

#### Mapping over the contents of Either objects

We don't need to immediately extract the values out once we've gotten an Either object back from a function. We can do more operations on the Either object without knowing its internal state. This is similar to how we can do operations the content of lists without knowing if the list is empty or how many items it contains.
```csharp
Either<Exception, int> exceptionOrInt = SomeFunction();
Either<Exception, string> exceptionOrString = exceptionOrInt.Select(x => x.ToString());
```
Here we have transformed the `Either<Exception, int>` to `Either<Exception, string>`. Notice how we at this point don't know if the value contained in the Either is an exception or an int. It doesn't really matter as far as we're concerned here. If it was an Exception then it will still be an Exception and if it was an int it will have been turned to a string.

Using the previous example of inverting numbers we can use this to do all kinds of operations on the results before we know if the function succeeded or not.
```csharp
string result = Invert(7)
    .Select(x => x + 8)
    .Select(x => x * 19)
    .Extract(
        err => err,
        x => "Result is: " x.ToString()
    );
```
#### Avoiding nested Either objects
But what if we want to call another function that can fail later down the chain? What will the result look like then? Well, rather ugly honestly.
```csharp
Either<string, double> either1 = Invert(x);
Either<string, Either<string, double>> either2 = either1.Select(Invert);
```
In order to get anything useful out of this we would have to nest the `Extract` method down through all the nested levels which gets messy.

Luckily, there is a way to get around this. We need a new method to handle this special case.
```csharp
public class Either<T, U>
{
    public Either<T, V> Then<V>(Func<U, Either<T, V>> f);
}
```
Here there are quite a lot of generic types to dig through. Firstly we have `T` and `U` which are the initial fail or success types of the Either object. Then we have a function `Func<U, Either<T, V>>` that operates on the type `U` and returns an `Either<T, V>`. This is also the return type of the `Then`.

This is how we get around the nesting of Either objects. If the initial Either object was in the success state we apply the new function to it's contents and return new Either object that that produces. If the Either object is in the fail state we just return the same state again just like we did in the `Select` case.

If we bring this new knowledge back to the example with the `Invert` function we can invert back and forth as much as we want without nesting our data types.

```csharp
Either<string, double> inverted1 =
    Invert(x)
    .Select(y => y - 2);

Either<string, double> inverted2 =
    inverted1
    .Then(Invert);
```

Armed with these abstractions we can now build entire functions that handle potential errors without having any special cases for when they occur.
```csharp
public static Either<string, double> DoSomeMath(double x)
{
    return Invert(x)
        .Select(y => y - 2)
        .Then(Invert)
        .Select(y => y * 2)
}
```
This function can fail in two places but the flow remains the same. As long as we are keeping our data inside the Either object we don't need to know or care if it has failed or not. It is all handled under the hood.
