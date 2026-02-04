import { NextRequest, NextResponse } from "next/server";

const AGENT_API_URL = process.env.AGENT_API_URL || "http://localhost:5149";

export async function GET(req: NextRequest) {
  const { searchParams } = req.nextUrl;
  const sessionId = searchParams.get("sessionId");
  const status = searchParams.get("status");

  if (!sessionId) {
    return NextResponse.json(
      { error: "sessionId is required" },
      { status: 400 }
    );
  }

  const params = new URLSearchParams({ sessionId });
  if (status) params.append("status", status);

  try {
    const response = await fetch(`${AGENT_API_URL}/api/campaigns?${params}`);
    
    if (!response.ok) {
      return NextResponse.json(
        { error: `Backend error: ${response.statusText}` },
        { status: response.status }
      );
    }

    const data = await response.json();
    return NextResponse.json(data);
  } catch (error) {
    console.error("Error fetching campaigns from backend:", error);
    return NextResponse.json(
      { error: "Failed to connect to backend API" },
      { status: 502 }
    );
  }
}
