import express from 'express';
import { runAnalysis } from '../services/playwrightService.js';

const router = express.Router();

router.post('/', async (req, res) => {
    const { url, fullPage } = req.body;
    if (!url) return res.status(400).json({ error: 'URL is required' });

    try {
        console.log(`📥 Analyze request for: ${url} | fullPage: ${fullPage === true}`);
        const result = await runAnalysis(url, fullPage === true);
        res.json(result);
    } catch (err) {
        console.error(`❌ Error analyzing ${url}:`, err);
        res.status(500).json({ error: 'Internal server error' });
    }
});

export default router;
