// auth/verification-email.service.ts
import { Injectable, BadRequestException } from "@nestjs/common";
import * as nodemailer from "nodemailer";

@Injectable()
export class EmailService {
  async sendVerificationEmail(to: string, token: string) {
    const transporter = nodemailer.createTransport({
      host: process.env.SMTP_HOST,
      port: 587,
      auth: {
        user: process.env.EMAIL_USER,
        pass: process.env.EMAIL_PASS,
      },
    });

    const verificationUrl = `${process.env.CLIENT_URL}/auth/verify-email?token=${token}`;

    const mailOptions = {
      from: process.env.EMAIL_USER,
      to,
      subject: "Please verify your email...",
      html: `<p>Hello, verify your email address by clicking on this</p>
        <br>
        <a href="${verificationUrl}">Click here to verify</a>
        `,
    };

    console.log(transporter, mailOptions);

    transporter.sendMail(mailOptions, (error, info) => {
      if (error) {
        console.log(error);
        throw new BadRequestException("Unable to send verification email");
      } else {
        console.log("Email sent: " + info.response);
      }
    });
  }
}
