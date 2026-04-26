#pragma once

#include <cstddef>
#include <limits>



//最大子数组（最大最小前缀和）（O(n)）（从本质上和Kadane算法是等价的，只是思考角度不同）
template <typename T>
void MaxSubArray(T* arr, int length, int& out_start, int& out_end)
{
	if (arr == nullptr || length <= 0)
	{
		out_start = -1;
		out_end = -1;
		return;
	}

	T prefix_sum = static_cast<T>(0);
	T min_prefix = static_cast<T>(0);
	T best_sum = std::numeric_limits<T>::lowest();
	int min_index = -1;
	int best_start = 0;
	int best_end = 0;

	for (int i = 0; i < length; ++i)
	{
		prefix_sum += arr[i];
		T current_sum = prefix_sum - min_prefix;

		if (current_sum > best_sum)
		{
			best_sum = current_sum;
			best_start = min_index + 1;
			best_end = i;
		}

		if (prefix_sum < min_prefix)
		{
			min_prefix = prefix_sum;
			min_index = i;
		}
	}

	out_start = best_start;
	out_end = best_end;
}



//最大子数组（Kadane算法 动态规划）（O(n)）
template <typename T>
void MaxSubArrayKadane(T* arr, int length, int& out_start, int& out_end)
{
	if (arr == nullptr || length <= 0)
	{
		out_start = -1;
		out_end = -1;
		return;
	}

	T current_sum = arr[0];
	T best_sum = arr[0];
	int current_start = 0;
	int best_start = 0;
	int best_end = 0;

	for (int i = 1; i < length; ++i)
	{
		if (current_sum < static_cast<T>(0))
		{
			current_sum = arr[i];
			current_start = i;
		}
		else
		{
			current_sum += arr[i];
		}

		if (current_sum > best_sum)
		{
			best_sum = current_sum;
			best_start = current_start;
			best_end = i;
		}
	}

	out_start = best_start;
	out_end = best_end;
}

template <typename T>
T MaxSubArrayDcCross(T* arr, int left, int mid, int right, int& out_start, int& out_end)
{
	T left_sum = std::numeric_limits<T>::lowest();
	T sum = static_cast<T>(0);
	int best_left = mid;

	for (int i = mid; i >= left; --i)
	{
		sum += arr[i];
		if (sum > left_sum)
		{
			left_sum = sum;
			best_left = i;
		}
	}

	T right_sum = std::numeric_limits<T>::lowest();
	sum = static_cast<T>(0);
	int best_right = mid + 1;

	for (int i = mid + 1; i <= right; ++i)
	{
		sum += arr[i];
		if (sum > right_sum)
		{
			right_sum = sum;
			best_right = i;
		}
	}

	out_start = best_left;
	out_end = best_right;
	return left_sum + right_sum;
}

template <typename T>
T MaxSubArrayDcCore(T* arr, int left, int right, int& out_start, int& out_end)
{
	if (left == right)
	{
		out_start = left;
		out_end = right;
		return arr[left];
	}

	int mid = left + (right - left) / 2;
	int left_start = left;
	int left_end = left;
	int right_start = right;
	int right_end = right;
	int cross_start = left;
	int cross_end = right;

	T left_sum = MaxSubArrayDcCore(arr, left, mid, left_start, left_end);
	T right_sum = MaxSubArrayDcCore(arr, mid + 1, right, right_start, right_end);
	T cross_sum = MaxSubArrayDcCross(arr, left, mid, right, cross_start, cross_end);

	if (left_sum >= right_sum && left_sum >= cross_sum)
	{
		out_start = left_start;
		out_end = left_end;
		return left_sum;
	}

	if (right_sum >= left_sum && right_sum >= cross_sum)
	{
		out_start = right_start;
		out_end = right_end;
		return right_sum;
	}

	out_start = cross_start;
	out_end = cross_end;
	return cross_sum;
}

//最大子数组(分治法) （O(nlogn)） 
template <typename T>
void MaxSubArrayDc(T* arr, int length, int& out_start, int& out_end)
{
	if (arr == nullptr || length <= 0)
	{
		out_start = -1;
		out_end = -1;
		return;
	}

	MaxSubArrayDcCore(arr, 0, length - 1, out_start, out_end);
}
