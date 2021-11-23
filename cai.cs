public class Calibration
{

    public Calibration()
    {
        this.nb_frames = 20;
        this.thresholds_left = new List<object>();
        this.thresholds_right = new List<object>();
    }

    // Returns true if the calibration is completed
    public virtual object is_complete()
    {
        return this.thresholds_left.Count >= this.nb_frames && this.thresholds_right.Count >= this.nb_frames;
    }

    /* Returns the threshold value for the given eye.
    //         Argument:
    //             side: Indicates whether it's the left eye (0) or the right eye (1)
    */
    public virtual object threshold(object side)
    {
        if (side == 0)
        {
            return Convert.ToInt32(this.thresholds_left.Sum() / this.thresholds_left.Count);
        }
        else if (side == 1)
        {
            return Convert.ToInt32(this.thresholds_right.Sum() / this.thresholds_right.Count);
        }
    }

    /* Returns the percentage of space that the iris takes up on
    //         the surface of the eye.
    //         Argument:
    //             frame (numpy.ndarray): Binarized iris frame
    */
    public static object iris_size(object frame)
    {
        frame = frame[5:: - 5, 5:: - 5];
        var _tup_1 = frame.shape[::2];
        var height = _tup_1.Item1;
        var width = _tup_1.Item2;
        var nb_pixels = height * width;
        var nb_blacks = nb_pixels - cv2.countNonZero(frame);
        return nb_blacks / nb_pixels;
    }

    /* Calculates the optimal threshold to binarize the
    //         frame for the given eye.
    //         Argument:
    //             eye_frame (numpy.ndarray): Frame of the eye to be analyzed
    */
    public static object find_best_threshold(object eye_frame)
    {
        var average_iris_size = 0.48;
        var trials = new Dictionary<object, object>
        {
        };
        foreach (var threshold in Enumerable.Range(0, Convert.ToInt32(Math.Ceiling(Convert.ToDouble(100 - 5) / 5))).Select(_x_1 => 5 + _x_1 * 5))
        {
            var iris_frame = Pupil.image_processing(eye_frame, threshold);
            trials[threshold] = Calibration.iris_size(iris_frame);
        }
        var _tup_1 = min(trials.items(), key: p => abs(p[1] - average_iris_size));
        var best_threshold = _tup_1.Item1;
        var iris_size = _tup_1.Item2;
        return best_threshold;
    }

    /* Improves calibration by taking into consideration the
    //         given image.
    //         Arguments:
    //             eye_frame (numpy.ndarray): Frame of the eye
    //             side: Indicates whether it's the left eye (0) or the right eye (1)
    */
    public virtual object evaluate(object eye_frame, object side)
    {
        var threshold = this.find_best_threshold(eye_frame);
        if (side == 0)
        {
            this.thresholds_left.append(threshold);
        }
        else if (side == 1)
        {
            this.thresholds_right.append(threshold);
        }
    }
}