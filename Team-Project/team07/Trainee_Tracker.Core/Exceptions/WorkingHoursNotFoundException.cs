namespace Trainee_Tracker.Core.Exceptions;

// Author: Marina Achner

/// <summary>
/// Exception that is thrown when requested working hours data cannot be found.
/// </summary>
public class WorkingHoursNotFoundException : Exception {
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkingHoursNotFoundException"/> class
    /// with a custom error message.
    /// </summary>
    /// <param name="errorMsg">
    /// The message describing the reason why the working hours could not be found.
    /// </param>
    public WorkingHoursNotFoundException(string errorMsg)
        : base(errorMsg) {
    }
}