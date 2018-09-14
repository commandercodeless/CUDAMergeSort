#include <stdio.h>
#include <math.h>
#include <stdlib.h>
#include <cuda.h>
#include <cuda_runtime.h>
#include <cuda_runtime_api.h>
#include <device_launch_parameters.h>
#include <device_functions.h>
#include <time.h>
#include "win-gettimeofday.h"


#define min(a,b) (a<b?a:b)
#define sum_squares(x) ((x*(x+1)*(2*x+1))/6)

const int k = 100;
const int N = k * 1024; //set data size
const int threadsPerBlock = 32;
const int blocksPerGrid = 1;

__device__ void doMerge(int A[], int start, int middle, int end, int B[]) {
	int i = start, j = middle;

	for (int k = start; k < end; k++) {
		if (i < middle && (j >= end || A[i] <= A[j])) {
			B[k] = A[i];
			i++;
		}
		else {
			B[k] = A[j];
			j++;
		}
	}
}

__device__ void doSplitMerge(int B[], int start, int end, int A[]) {
	if (end - start < 2) {
		return;
	}

	int middle = (end + start) / 2;

	doSplitMerge(A, start, middle, B);
	doSplitMerge(A, middle, end, B);

	doMerge(B, start, middle, end, A);
}

__global__ void mergeSort(int *a, int *b, int *elements, int *threads) {
	int block_id = blockIdx.x + gridDim.x * blockIdx.y;
	int thread_id = blockDim.x * block_id + threadIdx.x;  //calculate thread id

	int numberOfElements = N / (blocksPerGrid*threads[0]); //calculate how many elements this process must sort

	elements[thread_id] = numberOfElements;
	
	int startPoint = 0;
	for (int i = 0; i < thread_id; i++){
		startPoint += elements[i]; //calculate where this process must start in the array
	}

	doSplitMerge(b, startPoint, startPoint + numberOfElements, a); //sort the elements

	__syncthreads();
	
	for (int i = startPoint; i < startPoint + numberOfElements; i++){
		b[startPoint + i] = a[startPoint + i]; //make the backup array equal the uptodate array
	}
	__syncthreads();
}

int main(void) {
	int *a,
	int *dev_a,
	int *b,
	int *dev_b;
	int *cputhreads;
	int *elementsPerThread;
	int *threads;

	// allocate memory on the CPU
	a = (int*)malloc(N * sizeof(int));
	b = (int*)malloc(N * sizeof(int));
	cputhreads = (int*)malloc(sizeof(int));

	cputhreads[0] = threadsPerBlock; //set number of threads to constant variable
	
	// allocate memory on the GPU
	cudaMalloc((void**)&dev_a, N * sizeof(int));
	cudaMalloc((void**)&dev_b, N * sizeof(int));
	cudaMalloc((void**)&elementsPerThread, (threadsPerBlock * blocksPerGrid) * sizeof(int));
	cudaMalloc((void**)&threads,sizeof(int));

	// fill in the host memory with data
	for (int i = 0; i<N; i++) {
		a[i] = rand() % 1000;
		b[i] = a[i];
	}
	
	// start timer
	long long memory_start_time = start_timer();

	// copy the arrays a and b to the GPU
	cudaMemcpy(dev_a, a, N * sizeof(int), cudaMemcpyHostToDevice);
	cudaMemcpy(dev_b, b, N * sizeof(int), cudaMemcpyHostToDevice);

	cudaDeviceSynchronize();
	for (int i = cputhreads[0]; i > 0; i /= 2) {
		cputhreads[0] = i;
		cudaMemcpy(threads, cputhreads, sizeof(int), cudaMemcpyHostToDevice); //update GPU with number of threads
		mergeSort << <blocksPerGrid, cputhreads[0] >> > (dev_a, dev_b, elementsPerThread, threads);
		cudaDeviceSynchronize(); // make sure all threads have finished processing
	}

	// copy the array ’a’ back from the GPU to the CPU
	cudaMemcpy(a, dev_a, N * sizeof(int), cudaMemcpyDeviceToHost);

	// stop timer and print result
	stop_timer(memory_start_time, "\nGPU:\t Compute Sort");
			
	/* print the data */
	printf("Sorted array ");
	for (int i = 0; i < N; i++) {
		printf("%d, ", a[i]);
	}
	// free memory on the GPU
	cudaFree(dev_a);
	cudaFree(dev_b);
	cudaFree(elementsPerThread);
	cudaFree(threads);
	// free memory on the CPU
	free(a);
	free(b);
	free(cputhreads);

	getchar();
	return 0;
}