








float octave_noise(float2 pos) {
	float sum = 0;
	float scale = 1.0;

	for (int k = 0; k < octaves; k++) {
		float val = ofNoise(pos.x, pos.y);
		sum += val / scale;
		scale *= alpha;
		pos *= beta;
	}

	return sum;

}